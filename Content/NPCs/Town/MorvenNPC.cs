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
using Terraria.ModLoader.IO;
using System.IO;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.GameContent.Bestiary;
using NPCUtils;
using Terraria.Chat;
using PathOfTerraria.Common.NPCs.QuestMarkers;

namespace PathOfTerraria.Content.NPCs.Town;

[AutoloadHead]
public sealed class MorvenNPC : ModNPC, IQuestMarkerNPC, IOverheadDialogueNPC, IPathfindSyncingNPC
{
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	private readonly Pathfinder pathfinder = new(20);

	public Player FollowPlayer => Main.player[followPlayer];

	private float animCounter;
	private bool doPathing = false;
	private byte followPlayer;
	private bool teleportingToRavencrest = false;
	private bool abandoned = false;
	private short syncTimer = 0;

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
		
		NPC.TryEnableComponent<NPCHitEffects>(c => c.AddDust(new(DustID.Blood, 20)));
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

		return false;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override bool PreAI()
	{
		Tile tile = Main.tile[NPC.Center.ToTileCoordinates16()];

		if (teleportingToRavencrest)
		{
			NPC.Opacity -= 0.015f;
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
			CustomAttackHelper.CheckShootAttack(NPC, ref attackTime, ref attackingTime, ref attackDir, ref attackRotation);
			return false;
		}

		return true;
	}

	private void PathedMovement()
	{
		// Once every few seconds, sync the npc - bandaid on pathfinder in mp
		if (++syncTimer > 60)
		{
			NPC.netUpdate = true;
			syncTimer = 0;
		}

		Vector2 target = FollowPlayer.position;

		// If the player is too far away from the NPC, teleport the NPC and hurt him.
		// This reduces pathfinding load, especially if the player becomes fully blocked off somehow.
		if (Vector2.DistanceSquared(target, NPC.Center) > MathF.Pow(250 * 16, 2))
		{
			TeleportEffects();

			NPC.Center = target;
			NPC.SimpleStrikeNPC((int)(NPC.lifeMax / 3f), 0, true, noPlayerInteraction: true);

			TeleportEffects();

			string text = Language.GetTextValue("Mods.PathOfTerraria.NPCs.MorvenNPC.BubbleDialogue.Teleport." + Main.rand.Next(3));
			((IOverheadDialogueNPC)this).CurrentDialogue = new OverheadDialogueInstance(text, 300);
			return;
		}

		// Determines path using a slightly adjusted position and hitbox size.
		Point16 pathStart = (NPC.Top + new Vector2(8, 0)).ToTileCoordinates16();
		Point16 pathEnd = target.ToTileCoordinates16();
		pathfinder.CheckDrawPath(pathStart, pathEnd, new Vector2(NPC.width / 16f, NPC.height / 16f - 0.2f), null, new(-NPC.width / 2, 0));

		bool canPath = pathfinder.HasPath && pathfinder.Path.Count > 0;

		if (!canPath) // Retry pathing but slightly offset
		{
			pathStart = (NPC.Top - new Vector2(8, 0)).ToTileCoordinates16();
			pathEnd = target.ToTileCoordinates16();

			// Set RefreshTimer to 1 so it's 0 by the time it's checked & skips caching
			pathfinder.RefreshTimer = 1;
			pathfinder.CheckDrawPath(pathStart, pathEnd, new Vector2(NPC.width / 16f, NPC.height / 16f - 0.2f), null, new(-NPC.width / 2, 0));

			canPath = pathfinder.HasPath && pathfinder.Path.Count > 0;
		}

		if (canPath && NPC.DistanceSQ(target) > 160 * 160)
		{
			int index = pathfinder.Path.IndexOf(pathfinder.Path.MinBy(x => x.Position.ToVector2().DistanceSQ(NPC.position / 16f)));
			List<Pathfinder.FoundPoint> checkPoints = pathfinder.Path[^(Math.Min(pathfinder.Path.Count, 6))..];
			Vector2 direction = -AveragePathDirection(checkPoints) * 2;

			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction.X, 0.08f);
			NPC.velocity.Y += 0.1f;

			if (direction.Y < 0)
			{
				NPC.velocity.Y -= 0.6f;
				NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -5, 20);

				RocketBootDust();
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

		if (NPC.velocity.Y > 0 && !Collision.SolidCollision(NPC.BottomLeft, NPC.width, 60))
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

	private void TeleportEffects()
	{
		SoundEngine.PlaySound(SoundID.Item6, NPC.Center);

		for (int i = 0; i < 20; ++i)
		{
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Teleporter);
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
		button2 = !Quest.GetLocalPlayerInstance<EoWQuest>().CanBeStarted ? "" : Language.GetTextValue("Mods.PathOfTerraria.NPCs.Quest");
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (firstButton)
		{
			shopName = "Shop";
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

			Main.npcChatText = Language.GetTextValue("Mods.PathOfTerraria.NPCs.MorvenNPC.Dialogue.Rescue");
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest($"{PoTMod.ModName}/{nameof(EoWQuest)}");
		}
	}

	public override string GetChat()
	{
		if (abandoned)
		{
			abandoned = false;
			return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue.Abandoned");
		}

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
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.{Name}.Dialogue." + isFollow + Main.rand.Next(4));
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("pathing", doPathing);
		tag.Add("surfaceDialogue", surfaceDialogue);
		tag.Add("postOrbPreEoWDialogue", postOrbPreEoWDialogue);
		tag.Add("teleportingToRavencrest", teleportingToRavencrest);
	}

	public override void LoadData(TagCompound tag)
	{
		abandoned = tag.GetBool("pathing"); // If left while pathing, activate "abandoned" dialogue
		surfaceDialogue = tag.GetBool("surfaceDialogue");
		postOrbPreEoWDialogue = tag.GetBool("postOrbPreEowDialogue");
		teleportingToRavencrest = tag.GetBool("teleportingToRavencrest");
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		byte packed = 0;

		if (doPathing)
		{
			packed |= 0b_1;
		}

		if (surfaceDialogue)
		{
			packed |= 0b_10;
		}

		if (postOrbPreEoWDialogue)
		{
			packed |= 0b_100;
		}

		if (teleportingToRavencrest)
		{
			packed |= 0b_1000;
		}

		writer.Write(packed);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		byte packed = reader.ReadByte();

		doPathing = (packed & 0b_1) == 0b_1;
		surfaceDialogue = (packed & 0b_10) == 0b_10;
		postOrbPreEoWDialogue = (packed & 0b_100) == 0b_100;
		teleportingToRavencrest = (packed & 0b_1000) == 0b_1000;
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
			CustomAttackHelper.DrawShootAttack(NPC, screenPos, drawColor, attackRotation, ItemID.Handgun);
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
		followPlayer = player;
		doPathing = true;
	}

	public void DisablePathfinding()
	{
		doPathing = false;
	}
}
