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
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using System.IO;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using SubworldLibrary;
using PathOfTerraria.Common.Subworlds.BossDomains;
using Terraria.Chat;
using PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;
using PathOfTerraria.Common.NPCs.QuestMarkers;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public sealed class LloydNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC, IPathfindSyncingNPC
{
	const int HammerId = ItemID.PlatinumHammer;
	const int MaxHammerTime = 25;

	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	private readonly Pathfinder pathfinder = new(20);

	public Player FollowPlayer => Main.player[followPlayer];

	private float animCounter;
	private bool doPathing = false;
	private byte followPlayer;
	private bool abandoned = false;
	private short syncTimer = 0;
	private float wingTime = 0;
	private bool brainDialogue = true;

	// Sound timers
	private int walkTime = 0;
	private int rocketTime = 0;

	// Attack info
	private int attackTime = 0;
	private int attackingTime = 0;
	private int attackDir = 0;
	private float attackRotation = 0f;
	private int hammerTime = 0;

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Guide];

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
		
		NPC.TryEnableComponent<NPCHitEffects>(c => c.AddDust(new(DustID.Blood, 20)));
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override bool CheckDead()
	{
		NPC.active = false;
		TeleportEffects();

		Color color = Colors.RarityDarkRed;

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			Main.NewText(Language.GetText($"Mods.{PoTMod.ModName}.Misc.NPCLeftWithoutDying").Format(NPC.FullName), color.R, color.G, color.B);
		}
		else if (Main.netMode == NetmodeID.Server)
		{
			ChatHelper.BroadcastChatMessage(NetworkText.FromKey($"Mods.{PoTMod.ModName}.Misc.NPCLeftWithoutDying", NPC.FullName), color);
		}

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			ModContent.GetInstance<BoCDomainSystem>().DontSpawnLloyd = false;
		}

		return false;
	}

	public override bool PreAI()
	{
		if (NPC.hide) // When eaten by the maw to go to the Brain of Cthulhu domain, he will be invisible - stop movement so stuff like water doesn't defy invisibility
		{
			return false;
		}

		if (NPC.downedBoss2 && Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)].DistanceSQ(NPC.Center) > 2000 * 2000 
			&& Main.netMode != NetmodeID.MultiplayerClient)
		{
			ModContent.GetInstance<RavencrestSystem>().HasOverworldNPC.Add(FullName);

			NPC.active = false;
			return false;
		}

		if (doPathing && (!FollowPlayer.active || FollowPlayer.dead))
		{
			doPathing = false;
		}

		if (doPathing)
		{
			hammerTime--;

			PathedMovement();

			if (hammerTime <= 0)
			{
				CustomAttackHelper.CheckShootAttack(NPC, ref attackTime, ref attackingTime, ref attackDir, ref attackRotation);
			}

			return false;
		}

		return true;
	}

	private void PathedMovement()
	{
		// Once every second, sync the npc - bandaid on pathfinder in mp
		if (++syncTimer > 60)
		{
			NPC.netUpdate = true;
			syncTimer = 0;
		}

		Vector2 target = FollowPlayer.position;

		// If the player is too far away from the NPC, teleport the NPC and hurt him.
		// This reduces pathfinding load, especially if the player becomes fully blocked off somehow.
		if (Vector2.DistanceSquared(target, NPC.Center) > MathF.Pow(250 * 16, 2) && Main.netMode != NetmodeID.MultiplayerClient)
		{
			TeleportEffects();

			NPC.Center = target;
			NPC.netUpdate = true;
			NPC.netOffset = Vector2.Zero;
			NPC.velocity = Vector2.Zero;

			TeleportEffects();

			if (Main.netMode != NetmodeID.Server)
			{
				string text = Language.GetTextValue("Mods.PathOfTerraria.NPCs.LloydNPC.BubbleDialogue.Teleport." + Main.rand.Next(3));
				((IOverheadDialogueNPC)this).CurrentDialogue = new OverheadDialogueInstance(text, 300);
			}

			return;
		}

		ScanForHearts(ref target, out bool hasOrb);

		// Determines path using a slightly adjusted position and hitbox size.
		Point16 pathStart = NPC.Top.ToTileCoordinates16();
		Point16 pathEnd = target.ToTileCoordinates16();
		
		Vector2 pathSize = new(NPC.width / 16f * 0.8f, NPC.height / 16f * 0.8f);
		Vector2 pathOffset = new(-NPC.width / 2.5f, 0);

		pathfinder.CheckDrawPath(pathStart, pathEnd, pathSize, null, pathOffset);

		bool canPath = pathfinder.HasPath && pathfinder.Path.Count > 0;
		bool goDown = false;

		if (!canPath) // Retry pathing, not to the nearest orb
		{
			target = FollowPlayer.position;
			pathEnd = target.ToTileCoordinates16();

			// Resetting timer allows us to re-run pathfinding immediately.
			pathfinder.RefreshTimer = 1;
			pathfinder.CheckDrawPath(pathStart, pathEnd, pathSize, null, pathOffset);

			canPath = pathfinder.HasPath && pathfinder.Path.Count > 0;
			hasOrb = false;
		}

		float checkDist = hasOrb ? 40 * 40 : 160 * 160;

		if (canPath && NPC.DistanceSQ(target) > checkDist)
		{
			int index = pathfinder.Path.IndexOf(pathfinder.Path.MinBy(x => x.Position.ToVector2().DistanceSQ(NPC.position / 16f)));
			List<Pathfinder.FoundPoint> checkPoints = pathfinder.Path[^(Math.Min(pathfinder.Path.Count, 6))..];
			Vector2 direction = -AveragePathDirection(checkPoints) * 2;

			if (direction.Y > 0.5f)
			{
				direction.Y = 0.5f;
			}

			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction.X, 0.08f);
			NPC.velocity.Y += 0.1f;

			if (direction.Y < 0)
			{
				NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, direction.Y * 4, 0.3f);
				NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -14, 7);
			}
			else
			{
				goDown = true;
			}

			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

			// Debugging to show the calculated path.
			// This'll only show in DEBUG, for the local player.
#if DEBUG
			if (Main.myPlayer == followPlayer)
			{
				foreach (Pathfinder.FoundPoint item in pathfinder.Path)
				{
					var vel = Pathfinder.ToVector2(item.Direction);
					int id = checkPoints.Contains(item) ? item == checkPoints.Last() ? DustID.Poisoned : DustID.GreenFairy : DustID.YellowStarDust;
					var dust = Dust.NewDustPerfect(item.Position.ToWorldCoordinates(), id, vel * 2);
					dust.noGravity = true;
				}
			}
#endif
		}
		else
		{
			NPC.velocity *= 0.9f;
		}

		if (NPC.velocity.Y > 0 && !goDown)
		{
			NPC.velocity.Y -= 0.2f * (NPC.wet ? 0.5f : 1);
		}

		RunDustEffects();

		if (Math.Abs(NPC.velocity.X) > 0.1f)
		{
			NPC.direction = NPC.spriteDirection = MathF.Sign(NPC.velocity.X);
		}
	}

	private void ScanForHearts(ref Vector2 target, out bool hasOrb)
	{
		const int Distance = 35;

		Point16 pos = NPC.Center.ToTileCoordinates16();
		var orbPosition = new Point16(-1, -1);
		hasOrb = false;

		for (int i = pos.X - Distance; i < pos.X + Distance; i++) 
		{ 
			for (int j = pos.Y - Distance; j < pos.Y + Distance; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == TileID.ShadowOrbs && (orbPosition.X == -1 || 
					orbPosition.ToWorldCoordinates().DistanceSQ(NPC.Center) > new Vector2(i, j).ToWorldCoordinates().DistanceSQ(NPC.Center)))
				{
					orbPosition = new Point16(i, j);
				}
			}
		}

		if (orbPosition.X != -1)
		{
			target = new Vector2(orbPosition.X, orbPosition.Y).ToWorldCoordinates();
			hasOrb = true;

			if (hammerTime <= 0 && target.DistanceSQ(NPC.Center) < MathF.Pow(16 * 4, 2))
			{
				hammerTime = MaxHammerTime;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					WorldGen.KillTile(orbPosition.X, orbPosition.Y);

					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendTileSquare(-1, orbPosition.X, orbPosition.Y, 2, 2);
					}
				}

				int id = Math.Min(DisableEvilOrbBossSpawning.ActualOrbsSmashed, 3) - 1;
				string text = Language.GetTextValue("Mods.PathOfTerraria.NPCs.LloydNPC.BubbleDialogue.BreakHeart." + id);
				((IOverheadDialogueNPC)this).CurrentDialogue = new OverheadDialogueInstance(text, 300);
				return;
			}
		}
	}

	private void TeleportEffects()
	{
		// I don't really know what netOffset is but vanilla does this for Tim's teleport so
		NPC.position += NPC.netOffset;
		
		SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

		for (int i = 0; i < 20; ++i)
		{
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Firework_Red, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
		}

		NPC.position -= NPC.netOffset;
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
			Dust.NewDustPerfect(dustPos, DustID.Crimson, vel, Scale: Main.rand.NextFloat(1.2f, 1.8f));

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
			dir += Pathfinder.ToVector2(point.Direction) * new Vector2(3.2f, 1);
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
		projType = ProjectileID.BloodShot;
		attackDelay = 1;
	}

	public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
	{
		multiplier = 14f;
		randomOffset = 0.1f;
	}

	public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
	{
		Main.instance.LoadItem(ItemID.BloodRainBow);

		item = TextureAssets.Item[ItemID.BloodRainBow].Value;
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

		if (Quest.GetLocalPlayerInstance<BoCQuest>().Completed)
		{
			return; // No additional button once quest is done
		}

		button2 = Quest.GetLocalPlayerInstance<BoCQuest>().Active ? Language.GetTextValue($"Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.{(doPathing ? "Stay" : "Follow")}Button") 
			: Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
		}
		else
		{
			if (Quest.GetLocalPlayerInstance<BoCQuest>().Active)
			{
				if (doPathing)
				{
					if (Main.netMode == NetmodeID.SinglePlayer)
					{
						doPathing = false;
					}
					else
					{
						PathfindStateChangeHandler.Send((byte)Main.myPlayer, (byte)NPC.whoAmI, false);
					}

					Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.StayDialogue");
				}
				else
				{
					if (Main.netMode == NetmodeID.SinglePlayer)
					{
						followPlayer = 0;
						doPathing = true;
					}
					else
					{
						PathfindStateChangeHandler.Send((byte)Main.myPlayer, (byte)NPC.whoAmI, true);
					}

					Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.FollowAgain");
				}
			}
			else
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					followPlayer = 0;
					doPathing = true;
				}
				else
				{
					PathfindStateChangeHandler.Send((byte)Main.myPlayer, (byte)NPC.whoAmI, true);
				}

				Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.Help");
				Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(BoCQuest)}");
			}
		}
	}

	public override string GetChat()
	{
		if (SubworldSystem.Current is BrainDomain && brainDialogue)
		{
			brainDialogue = false;
			return Language.GetTextValue("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.InDomain");
		}

		string isFollow = SubworldSystem.Current is BrainDomain ? "Domain" : doPathing ? "Follow" : "Common";
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue." + isFollow + Main.rand.Next(4));
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("pathing", doPathing);
	}

	public override void LoadData(TagCompound tag)
	{
		abandoned = tag.GetBool("pathing"); // If left while pathing, activate "abandoned" dialogue
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(doPathing);
		writer.Write(followPlayer);
		//pathfinder.SendPath(writer);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		doPathing = reader.ReadBoolean();
		followPlayer = reader.ReadByte();
		//pathfinder.ReadPath(reader);
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

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (!NPC.IsABestiaryIconDummy && !Collision.SolidCollision(NPC.BottomLeft, NPC.width, 6))
		{
			Main.instance.LoadWings(2);

			Texture2D wings = TextureAssets.Wings[2].Value;
			wingTime += Math.Abs(NPC.velocity.Y) / 10f;

			int frameHeight = wings.Height / 4;
			int frameNum = NPC.velocity.Y > -0.5f ? 2 : (int)(wingTime % 16f / 4f);
			var frame = new Rectangle(0, frameHeight * frameNum, wings.Width, frameHeight);
			SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 wingPos = NPC.Center - screenPos - new Vector2(NPC.spriteDirection * 4, NPC.gfxOffY - 2);
			
			Main.EntitySpriteDraw(wings, wingPos, frame, drawColor, NPC.rotation, frame.Size() / 2f, 1f, effects);
		}

		return true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (attackingTime > 0 && hammerTime <= 0)
		{
			CustomAttackHelper.DrawShootAttack(NPC, screenPos, drawColor, attackRotation, ItemID.Handgun);
		}
		else if (hammerTime > 0)
		{
			float scale = 1f;
			int horizontalOff = 0;

			Main.GetItemDrawFrame(HammerId, out Texture2D tex, out Rectangle value2);

			SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			float rot = hammerTime / (float)MaxHammerTime * (MathHelper.PiOver4 * 3) * -NPC.spriteDirection + MathHelper.PiOver4 * 1.5f;
			var origin = new Vector2(0, tex.Height);

			if (NPC.spriteDirection == -1)
			{
				rot -= MathHelper.Pi * 0.75f;
				origin = new Vector2(tex.Width, tex.Height);
			}

			Main.EntitySpriteDraw(tex, NPC.Center - screenPos - new Vector2(horizontalOff, NPC.gfxOffY), null, drawColor, rot, origin, scale, effects);
		}
	}

	public bool HasQuestMarker(out Quest quest)
	{
		quest = Quest.GetLocalPlayerInstance<EoCQuest>();
		return !quest.Completed;
	}

	bool IOverheadDialogueNPC.ShowDialogue()
	{
		return Main.rand.NextBool(doPathing ? attackingTime > 0 ? 20 : 900 : 1200);
	}

	string IOverheadDialogueNPC.GetDialogue()
	{
		string baseNPC = $"Mods.PathOfTerraria.NPCs.{Name}.BubbleDialogue.";

		if (SubworldSystem.Current is BrainDomain domain)
		{
			return Language.GetTextValue(baseNPC + (domain.BossSpawned && !NPC.AnyNPCs(NPCID.BrainofCthulhu) ? "DomainBossDead." : "InDomain.") + Main.rand.Next(3));
		}

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

	public void EnablePathfinding(byte player)
	{
		doPathing = true;
		followPlayer = player;
	}

	public void DisablePathfinding()
	{
		doPathing = false;
	}
}
