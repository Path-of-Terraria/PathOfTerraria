using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using PathOfTerraria.Common.NPCs.Pathfinding;
using System.Linq;
using System.Collections.Generic;
using PathOfTerraria.Content.Tiles.Town;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using Terraria.DataStructures;
using Terraria.Audio;
using SubworldLibrary;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public sealed class MorvenNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC
{
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	private readonly Pathfinder pathfinder = new(20);

	public Player FollowPlayer => Main.player[followPlayer];

	private float animCounter;
	private bool doPathing = false;
	private byte followPlayer;
	private bool teleportingToRavencrest = false;

	// Sound timers
	private int walkTime = 0;
	private int rocketTime = 0;

	// Dialogue stuff
	private bool surfaceDialogue = true;
	private bool postOrbPreEoWDialogue = true;

	// Attack info
	private int attackTime = 0;
	private int attackingTime = 0;
	private int attackDir = 0;
	private float attackRotation = 0f;

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 25;

		NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
		NPCID.Sets.AttackFrameCount[NPC.type] = 4;
		NPCID.Sets.DangerDetectRange[NPC.type] = 800;
		NPCID.Sets.AttackType[NPC.type] = 1;
		NPCID.Sets.AttackTime[NPC.type] = 16;
		NPCID.Sets.AttackAverageChance[NPC.type] = 3;
		NPCID.Sets.NoTownNPCHappiness[Type] = true;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.ArmsDealer);
		NPC.townNPC = true;
		NPC.friendly = true;
		NPC.aiStyle = 7;
		NPC.defense = 30;
		NPC.lifeMax = 250;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = 0.5f;
		AnimationType = NPCID.Guide;
		
		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new($"{PoTMod.ModName}/{Name}_0", 1));
				c.AddGore(new($"{PoTMod.ModName}/{Name}_1", 2));
				c.AddGore(new($"{PoTMod.ModName}/{Name}_2", 2));
				
				c.AddDust(new(DustID.Blood, 20));
			}
		);
	}

	public override bool PreAI()
	{
		Tile tile = Main.tile[NPC.Center.ToTileCoordinates16()];

		if (teleportingToRavencrest)
		{
			NPC.Opacity -= 0.0015f;
			NPC.velocity *= 0.85f;

			if (NPC.Opacity <= 0)
			{
				NPC.active = false;

				ModContent.GetInstance<RavencrestSystem>().HasOverworldNPC.Add(FullName);
			}

			return false;
		}
		else if (tile.HasTile && tile.TileType == ModContent.TileType<RavenStatue>())
		{
			teleportingToRavencrest = true;
		}

		if (doPathing)
		{
			PathedMovement();
			CheckAttack();
			return false;
		}

		return true;
	}

	private void CheckAttack()
	{
		NPC first = null;
		int checkDist = NPCID.Sets.DangerDetectRange[NPC.type] * NPCID.Sets.DangerDetectRange[NPC.type];

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.CanBeChasedBy() && npc.whoAmI != NPC.whoAmI && npc.DistanceSQ(NPC.Center) < checkDist && 
				(first is null || first.DistanceSQ(NPC.Center) > npc.DistanceSQ(NPC.Center)) && Collision.CanHit(NPC, npc))
			{
				first = npc;
				break;
			}
		}

		if (--attackTime < 0 && first is not null)
		{
			int atkCool = 10;
			int randCool = 0;
			NPCLoader.TownNPCAttackCooldown(NPC, ref atkCool, ref randCool);

			float velMul = 1f;
			float throwaway = 0;
			float offset = 0;
			NPCLoader.TownNPCAttackProjSpeed(NPC, ref velMul, ref throwaway, ref offset);

			int projId = 0;
			int thrwInt = 0;
			NPCLoader.TownNPCAttackProj(NPC, ref projId, ref thrwInt);

			int damage = 0;
			float knockback = 0;
			NPCLoader.TownNPCAttackStrength(NPC, ref damage, ref knockback);

			Vector2 velocity = NPC.DirectionTo(first.Center).RotatedByRandom(offset * 0.2f) * velMul;
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, projId, damage, knockback, Main.myPlayer);

			attackingTime = NPCID.Sets.AttackTime[NPC.type];
			attackRotation = velocity.ToRotation();
			attackTime = atkCool * 60 + Main.rand.Next(randCool * 60);
			attackTime /= 8;
			attackDir = Math.Sign(velocity.X);

			SoundEngine.PlaySound(SoundID.Item11, NPC.Center);
		}

		if (--attackingTime > 0)
		{
			NPC.direction = NPC.spriteDirection = attackDir;
		}
	}

	private void PathedMovement()
	{
		Vector2 target = FollowPlayer.Center - new Vector2(FollowPlayer.direction * 32, 18);
		Vector2 secondTarget = FollowPlayer.Center - new Vector2(-FollowPlayer.direction * 32, 18);

		while (Collision.SolidCollision(target, 16, 16))
		{
			target = Vector2.Lerp(target, secondTarget, 0.1f);

			if (target.DistanceSQ(secondTarget) < 2 * 2)
			{
				break;
			}
		}

		Point16 pathStart = (NPC.Top + new Vector2(8, 0)).ToTileCoordinates16();
		Point16 pathEnd = target.ToTileCoordinates16();
		pathfinder.CheckDrawPath(pathStart, pathEnd, new Vector2(NPC.width / 16f, NPC.height / 16f - 0.1f), null, new(-NPC.width / 2, 0));

		bool canPath = pathfinder.HasPath && pathfinder.Path.Count > 0;

		if (!canPath) // Retry pathing but slightly offset
		{
			pathStart = (NPC.Top - new Vector2(8, 0)).ToTileCoordinates16();
			pathEnd = target.ToTileCoordinates16();
			pathfinder.CheckDrawPath(pathStart, pathEnd, new Vector2(NPC.width / 16f, NPC.height / 16f - 0.1f), null, new(-NPC.width / 2, 0));

			canPath = pathfinder.HasPath && pathfinder.Path.Count > 0;
		}

		if (canPath && NPC.DistanceSQ(target) > 160 * 160)
		{
			int index = pathfinder.Path.IndexOf(pathfinder.Path.MinBy(x => x.Position.ToVector2().DistanceSQ(NPC.position / 16f)));
			List<Pathfinder.FoundPoint> checkPoints = pathfinder.Path[^(Math.Min(pathfinder.Path.Count, 6))..];
			Vector2 direction = -AveragePathDirection(checkPoints) * 2;

			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction.X, 0.05f);
			NPC.velocity.Y += 0.1f;

			if (direction.Y < 0)
			{
				NPC.velocity.Y -= 0.6f;
				NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -5, 20);

				RocketBootDust();
			}

			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

			foreach (Pathfinder.FoundPoint item in pathfinder.Path)
			{
				var vel = Pathfinder.ToVector(item.Direction).ToVector2();
				int id = checkPoints.Contains(item) ? item == checkPoints.Last() ? DustID.Poisoned : DustID.GreenFairy : DustID.YellowStarDust;
				var dust = Dust.NewDustPerfect(item.Position.ToWorldCoordinates(), id, vel * 2);
				dust.noGravity = true;
			}
		}
		else
		{
			NPC.velocity *= 0.9f;
		}

		if (NPC.velocity.Y > 0 && !Collision.SolidCollision(NPC.BottomLeft, NPC.width, 90))
		{
			NPC.velocity.Y -= canPath ? 0.3f : 0.2f;
			RocketBootDust();
		}

		RunDustEffects();

		if (Math.Abs(NPC.velocity.X) > 0.1f)
		{
			NPC.direction = NPC.spriteDirection = MathF.Sign(NPC.velocity.X);
		}
	}

	private void RunDustEffects()
	{
		bool ground = NPC.velocity.Y == 0 || NPC.velocity.Y == 0.1f;
		int chance = 6 - (int)Math.Min(5, Math.Abs(NPC.velocity.X));

		if (ground && Collision.SolidCollision(NPC.BottomLeft, NPC.width, 6) && (chance < 1 || Main.rand.NextBool(chance)) && Math.Abs(NPC.velocity.X) > 2)
		{
			Point floorPos = NPC.Center.ToTileCoordinates();

			while (!WorldGen.SolidOrSlopedTile(floorPos.X, floorPos.Y))
			{
				floorPos.Y++;
			}

			Vector2 dustPos = floorPos.ToWorldCoordinates() + new Vector2(Main.rand.NextFloat(NPC.width), NPC.gfxOffY - 8);
			Vector2 vel = new Vector2(-NPC.velocity.X * 0.1f, Main.rand.NextFloat(-0.5f, 0.5f)).RotatedByRandom(0.2f);
			Dust.NewDustPerfect(dustPos, DustID.Cloud, vel, Scale: Main.rand.NextFloat(1.2f, 1.8f));

			walkTime += (int)Math.Abs(NPC.velocity.X);

			if (walkTime > 40)
			{
				SoundEngine.PlaySound(SoundID.Run with { Volume = 0.8f, PitchRange = (-0.1f, 0.1f) }, NPC.Bottom);

				walkTime = 0;
			}
		}
	}

	private void RocketBootDust()
	{
		if (!Main.rand.NextBool(3))
		{
			for (int i = 0; i < 2; ++i)
			{
				var vector = new Vector2((i % 2 == 0 ? -1 : 1) - NPC.velocity.X * 0.3f, 2f - NPC.velocity.Y * 0.3f);
				Dust.NewDustPerfect(NPC.BottomLeft + new Vector2(Main.rand.NextFloat(NPC.width), 0), DustID.Torch, vector, 0, default, Main.rand.NextFloat(2, 2.5f));
			}
		}

		if (rocketTime++ >= 20)
		{
			SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.6f, PitchRange = (-0.1f, 0.1f) }, NPC.Bottom);

			rocketTime = 0;
		}
	}

	private static Vector2 AveragePathDirection(List<Pathfinder.FoundPoint> foundPoints)
	{
		Vector2 dir = Vector2.Zero;

		foreach (Pathfinder.FoundPoint point in foundPoints)
		{
			dir += Pathfinder.ToVector(point.Direction).ToVector2() * new Vector2(3f, 1);
		}

		return dir / foundPoints.Count;
	}

	public override void TownNPCAttackStrength(ref int damage, ref float knockback)
	{
		damage = 20;
		knockback = 3f;
	}

	public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
	{
		cooldown = 5;
		randExtraCooldown = 3;
	}

	public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
	{
		projType = ProjectileID.Bullet;
		attackDelay = 1;
	}

	public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
	{
		multiplier = 11f;
		randomOffset = 0.1f;
	}

	public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
	{
		Main.instance.LoadItem(ItemID.Handgun);

		item = TextureAssets.Item[ItemID.Handgun].Value;
		itemFrame = item.Frame(1, 1, 0, 0);
		horizontalHoldoutOffset = -8;
	}

	public override ITownNPCProfile TownNPCProfile()
	{
		return this.GetDefaultProfile();
	}

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Language.GetTextValue("LegacyInterface.28");
		button2 = /*!ModContent.GetInstance<EoWQuest>().CanBeStarted ? "" :*/ Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
		}
		else
		{
			followPlayer = 0;
			doPathing = true;
			NPC.netUpdate = true;
			
			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.MorvenNPC.Dialogue.Rescue");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(EoWQuest)}");
		}
	}

	public override string GetChat()
	{
		if (NPC.Center.Y / 16 < Main.worldSurface && surfaceDialogue && SubworldSystem.Current is null)
		{
			surfaceDialogue = false;
			return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Aboveground");
		}

		if (ModContent.GetInstance<RavencrestSystem>().HasOverworldNPC.Contains(FullName) && postOrbPreEoWDialogue && DisableEvilOrbBossSpawning.ActualOrbsSmashed > 2)
		{
			postOrbPreEoWDialogue = false;
			return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.BeforeBeatingEoW");
		}

		string isFollow = doPathing ? "Follow" : "Common";
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue." + isFollow + + Main.rand.Next(4));
	}

	public override void FindFrame(int frameHeight)
	{
		if (!NPC.IsABestiaryIconDummy)
		{
			return;
		}

		animCounter += 0.25f;

		if (animCounter >= 16)
		{
			animCounter = 2;
		}
		else if (animCounter < 2)
		{
			animCounter = 2;
		}

		int frame = (int)animCounter;
		NPC.frame.Y = frame * frameHeight;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (attackingTime > 0)
		{
			Texture2D item = null;
			Rectangle frame = default;
			float scale = 1f;
			int horizontalOff = 0;
			int itemType = ItemID.Handgun;

			Vector2 vector11 = Main.DrawPlayerItemPos(1f, itemType);
			Main.GetItemDrawFrame(itemType, out Texture2D itemTexture, out Rectangle value2);
			int num4 = (int)vector11.X - 4;
			var origin2 = new Vector2(-num4, value2.Height / 2);

			if (NPC.spriteDirection == -1)
			{
				origin2 = new Vector2(value2.Width + num4, value2.Height / 2);
			}

			NPCLoader.DrawTownAttackGun(NPC, ref item, ref frame, ref scale, ref horizontalOff);
			SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			float rot = attackRotation;

			if (NPC.spriteDirection == -1)
			{
				rot -= MathHelper.Pi;
			}

			Main.EntitySpriteDraw(item, NPC.Center - screenPos - new Vector2(horizontalOff, NPC.gfxOffY), frame, drawColor, rot, origin2, scale, effects);
		}
	}

	public bool HasQuestMarker(out Quest quest)
	{
		quest = ModContent.GetInstance<EoCQuest>();
		return !quest.Completed;
	}

	bool IOverheadDialogueNPC.ShowDialogue()
	{
		return Main.rand.NextBool(doPathing ? attackingTime > 0 ? 20 : 900 : 1200);
	}

	string IOverheadDialogueNPC.GetDialogue()
	{
		string baseNPC = $"Mods.PathOfTerraria.NPCs.{Name}.BubbleDialogue.";

		if (doPathing)
		{
			return Language.GetTextValue(baseNPC + (attackingTime > 0 ? "EscortAttack." : "Escort.") + Main.rand.Next(3));
		}

		if (Main.invasionType == InvasionID.GoblinArmy)
		{
			return Language.GetTextValue(baseNPC + "Goblins." + Main.rand.Next(3));
		}

		if (Main.IsItRaining)
		{
			return Language.GetTextValue(baseNPC + "Rain." + Main.rand.Next(3));
		}

		if (Main.dayTime)
		{
			return Language.GetTextValue(baseNPC + "Day." + Main.rand.Next(3));
		}

		return Language.GetTextValue(baseNPC + "Night." + Main.rand.Next(3));
	}
}
