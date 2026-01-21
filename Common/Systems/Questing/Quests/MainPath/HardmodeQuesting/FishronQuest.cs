using System.Collections.Generic;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using SubworldLibrary;
using Terraria.ID;
using Terraria.Localization;


#nullable enable

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class FishronQuest() : Quest
{
	public static int GetRiftSide()
	{
		// Opposite of Calamity Mod's sulphur sea placement, which is always same as the dungeon's.
		bool sulphurSeaIsAtLeftSide = Main.dungeonX < Main.maxTilesX / 2;
		return sulphurSeaIsAtLeftSide ? +1 : -1;
	}

	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<FishermanNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
			{
				p.GetModPlayer<ExpModPlayer>().Exp += 30000;
			},
			"30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ParallelQuestStep("Start", [
				new CollectCount("Coral", ItemID.Coral, 10),
				//TODO: Change to be 5 of ANY fish.
				new CollectCount("Fish", ItemID.Fish, 5),
				new KillCount("Kill", npc => npc.netID is NPCID.Shark or NPCID.Crab or NPCID.BlueJellyfish or NPCID.GreenJellyfish or NPCID.PinkJellyfish, 10, this.GetLocalization("OceanEnemies"))
			], Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue1")),

			new InteractWithNPC("Talk", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue2")),
			
			new InteractWithNPC("TruffleWorm", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue3"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue3"),
				[
					new GiveItem(1, ItemID.TruffleWorm),
				],
				//TODO: This needs to be changed to the rift in the ocean, throwing a truffle worm in it will drag you into it, teleporting to the fishrom map.
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<FishronMap>())),

			new ConditionCheck("Domain", _ => SubworldSystem.Current is FishronDomain, 1, this.GetLocalization("EnterDomain")),
			
			new ConditionCheck("Boss", _ => BossTracker.DownedInDomain<FishronDomain>(NPCID.DukeFishron), 1, this.GetLocalization("Boss")),
	
			new InteractWithNPC("Finish", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue4"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue4"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest golemQuest = GetLocalPlayerInstance<GolemQuest>();
		return golemQuest.Completed && NPC.downedGolemBoss;
	}
}

// Simply spawns and despawns the rift.
file sealed class FishronQuestSystem : ModSystem
{
	public override void PreUpdateWorld()
	{
		int riftType = ModContent.ProjectileType<UnderwaterRift>();
		bool riftShouldExist = false;
		foreach (Player player in Main.ActivePlayers)
		{
			if (Quest.PlayerHasQuest<FishronQuest>(player.whoAmI))
			{
				riftShouldExist = true;
				break;
			}
		}

		Projectile? existingRift = null;
		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.type == riftType)
			{
				existingRift = projectile;
				break;
			}
		}

		bool riftExists = existingRift != null;
		if (riftExists != riftShouldExist)
		{
			if (riftShouldExist)
			{
				SpawnRift();
			}
			else
			{
				existingRift!.active = false;
				existingRift.netUpdate = true;
			}
		}
	}

	private static void SpawnRift()
	{
		int riftDir = FishronQuest.GetRiftSide();
		bool leftSide = riftDir == -1;
		int xFarthest = (int)((leftSide ? Main.leftWorld : Main.rightWorld) / TileUtils.TileSizeInPixels) - (96 * riftDir);
		int xClosest = xFarthest - (32 * riftDir);

		for (int i = 0; i < 50; i++)
		{
			int xBase = Main.rand.Next(leftSide ? xFarthest : xClosest, leftSide ? xClosest : xFarthest);
			int yBase = (int)(Main.maxTilesY * 0.35f / 16f);
			int yLimit = yBase + 400;

			for (; yBase < yLimit && yBase < Main.maxTilesY; yBase++)
			{
				if (WorldUtilities.SolidTile(xBase, yBase))
				{
					break;
				}
			}

			if (yBase == yLimit) { continue; }

			Rectangle furtherSearchRect = new Rectangle(xBase, yBase, 0, 0).Inflated(32, 32);

			if (EnemySpawning.TryFindingSpawnPosition(out Vector2 spawnPoint, new()
			{
				Area = furtherSearchRect,
				CollisionSize = new Point(4, 12).ToWorldCoordinates().ToPoint(),
				OnGround = true,
				MinDistanceFromEnemies = 0f,
				MinDistanceFromPlayers = 1500f,
				MaxSearchAttempts = 16,
			}))
			{
				Vector2 position = spawnPoint + new Vector2(0f, -64f);
				Projectile.NewProjectile(null, position, default, ModContent.ProjectileType<UnderwaterRift>(), 0, 0f);
				break;
			}
		}
	}
}