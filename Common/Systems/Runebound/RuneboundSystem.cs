using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Items.Currency.Runebound;
using PathOfTerraria.Content.NPCs.Runebound;
using PathOfTerraria.Common.Systems.Scarabs;
using PathOfTerraria.Utilities.Terraria;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Runebound;

internal sealed class RuneboundSystem : ModSystem
{
	private sealed class EncounterData
	{
		public int Id;
		public int MonolithIndex;
		public RuneboundFamily Family;
		public RunestoneGrade Grade;
		public bool Tutorial;
		public bool MapEncounter;
		public bool Active;
		public readonly HashSet<int> EnemyIndices = [];
		public readonly HashSet<int> Participants = [];
	}

	private const int AmbientSpawnDelay = 60 * 90;
	private const float ParticipationRange = 1600f;
	private static readonly Dictionary<int, EncounterData> Encounters = [];
	private static int nextEncounterId;
	private static int ambientSpawnTimer;
	private static int mapEncountersCompleted;
	private static int mapSpawnAttemptTimer;
	private static int menagerieCompletedFamilyMask;

#if DEBUG
	internal readonly record struct DebugEncounterInfo(int Id, RuneboundFamily Family, RunestoneGrade Grade,
		bool Tutorial, bool MapEncounter, bool Active, int EnemyCount, int ParticipantCount, Vector2 Position);

	internal readonly record struct DebugMapInfo(int TargetCount, int CompletedCount, int ActiveCount,
		int RemainingCount, int SpawnAttemptTimer, DebugEncounterInfo[] Encounters);

	internal static DebugMapInfo GetDebugMapInfo()
	{
		DebugEncounterInfo[] encounters = Encounters.Values.Select(encounter =>
		{
			Vector2 position = encounter.MonolithIndex >= 0 && encounter.MonolithIndex < Main.maxNPCs
				? Main.npc[encounter.MonolithIndex].Center
				: Vector2.Zero;

			return new DebugEncounterInfo(encounter.Id, encounter.Family, encounter.Grade, encounter.Tutorial,
				encounter.MapEncounter, encounter.Active, encounter.EnemyIndices.Count, encounter.Participants.Count, position);
		}).ToArray();

		int target = IsExplorationMap() ? GetGuaranteedMapEncounterCount() : 0;
		int active = encounters.Count(encounter => encounter.MapEncounter);
		return new DebugMapInfo(target, mapEncountersCompleted, active,
			Math.Max(0, target - mapEncountersCompleted - active), mapSpawnAttemptTimer, encounters);
	}
#endif

	public override void ClearWorld()
	{
		Encounters.Clear();
		nextEncounterId = 0;
		ambientSpawnTimer = AmbientSpawnDelay;
		mapEncountersCompleted = 0;
		mapSpawnAttemptTimer = 120;
		menagerieCompletedFamilyMask = 0;
		RuneboundSpawnContext.Clear();
	}

	public override void SaveWorldData(TagCompound tag)
	{
		if (IsExplorationMap())
		{
			tag["runeboundMapEncountersCompleted"] = mapEncountersCompleted;
			tag["runeboundMenagerieCompletedFamilyMask"] = menagerieCompletedFamilyMask;
		}
	}

	public override void LoadWorldData(TagCompound tag)
	{
		mapEncountersCompleted = Math.Max(0, tag.GetInt("runeboundMapEncountersCompleted"));
		menagerieCompletedFamilyMask = tag.GetInt("runeboundMenagerieCompletedFamilyMask") & 0b111;
	}

	public override void PostUpdateWorld()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		RemoveInvalidEncounters();

		if (IsExplorationMap())
		{
			UpdateGuaranteedMapEncounters();
			return;
		}

		if (SubworldSystem.Current is null && TryGetTutorialPlayer(out Player tutorialPlayer))
		{
			if (!HasActiveEncounter() && Main.GameUpdateCount % 60 == 0)
			{
				TrySpawnEncounter(tutorialPlayer, RuneboundFamily.Vigor, RunestoneGrade.Faint, tutorial: true);
			}

			return;
		}

		if (!CanSpawnAmbientEncounter() || HasActiveEncounter())
		{
			return;
		}

		if (ambientSpawnTimer-- > 0)
		{
			return;
		}

		ambientSpawnTimer = AmbientSpawnDelay;
		Player target = null;
		foreach (Player candidate in Main.ActivePlayers)
		{
			if (!candidate.dead && candidate.GetModPlayer<RuneboundPlayer>().LearnedMechanic)
			{
				target = candidate;
				break;
			}
		}

		if (target != null)
		{
			TrySpawnEncounter(target, RollFamily(), RollGrade(), tutorial: false);
		}
	}

	private static bool IsExplorationMap()
	{
		return SubworldSystem.Current is MappingWorld and IExplorationWorld and not RavencrestSubworld;
	}

	private static void UpdateGuaranteedMapEncounters()
	{
		int activeMapEncounters = Encounters.Values.Count(encounter => encounter.MapEncounter);
		int remainingToSpawn = GetGuaranteedMapEncounterCount() - mapEncountersCompleted - activeMapEncounters;

		if (remainingToSpawn <= 0 || mapSpawnAttemptTimer-- > 0)
		{
			return;
		}

		Player target = null;
		foreach (Player candidate in Main.ActivePlayers)
		{
			if (!candidate.dead)
			{
				target = candidate;
				break;
			}
		}

		if (target is null)
		{
			mapSpawnAttemptTimer = 60;
			return;
		}

		bool spawned = TrySpawnEncounter(target, RollFamily(), RollGrade(), tutorial: false, mapEncounter: true);
		mapSpawnAttemptTimer = spawned ? 15 : 60;
	}

	private static int GetGuaranteedMapEncounterCount()
	{
		int scarabBonus = ScarabSystem.FindFamily(ScarabFamily.Binding) is { } binding
			? binding.Kind == ScarabKind.RunicMenagerie ? 3 : ScarabCatalog.GetPower(binding.Grade)
			: 0;
		return 2 + (MappingWorld.MapTier >= 5 ? 1 : 0) + (MappingWorld.MapTier >= 10 ? 1 : 0) + scarabBonus;
	}

	private static bool TryGetTutorialPlayer(out Player player)
	{
		player = null;
		foreach (Player candidate in Main.ActivePlayers)
		{
			RuneboundPlayer progress = candidate.GetModPlayer<RuneboundPlayer>();
			if (!candidate.dead && progress.TutorialEncounterRequested && !progress.TutorialEncounterCompleted
				&& Quest.PlayerHasQuest<TheFirstBindingQuest>(candidate.whoAmI))
			{
				player = candidate;
				break;
			}
		}

		return player != null;
	}

	private static bool CanSpawnAmbientEncounter()
	{
		if (!Main.hardMode || Main.invasionType != 0)
		{
			return false;
		}

		return SubworldSystem.Current is null or (MappingWorld and not RavencrestSubworld);
	}

	private static bool HasActiveEncounter()
	{
		return Encounters.Count != 0;
	}

	private static void RemoveInvalidEncounters()
	{
		foreach (EncounterData encounter in Encounters.Values.ToArray())
		{
			if (!encounter.Active)
			{
				continue;
			}

			encounter.EnemyIndices.RemoveWhere(index => index < 0 || index >= Main.maxNPCs || !Main.npc[index].active);
			if (encounter.EnemyIndices.Count == 0)
			{
				// Normal kills complete synchronously through NotifyEnemyKilled. Reaching this path
				// means the NPCs vanished without a kill, so never grant rewards for it.
				Encounters.Remove(encounter.Id);
			}
		}

		foreach (EncounterData encounter in Encounters.Values.Where(value => !value.Active
			&& (value.MonolithIndex < 0 || value.MonolithIndex >= Main.maxNPCs
				|| !Main.npc[value.MonolithIndex].active)).ToArray())
		{
			// Defensive cleanup for a sealed pack whose monolith was removed by another system.
			foreach (int npcIndex in encounter.EnemyIndices)
			{
				if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
				{
					Main.npc[npcIndex].active = false;
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
				}
			}

			Encounters.Remove(encounter.Id);
		}
	}

	private static bool TrySpawnEncounter(Player target, RuneboundFamily family, RunestoneGrade grade, bool tutorial,
		bool mapEncounter = false)
	{
		bool foundPosition = mapEncounter ? TryFindMapSpawnPosition(out Vector2 position) : TryFindSpawnPosition(target, out position);
		if (!foundPosition)
		{
			return false;
		}

		var source = new EntitySource_Misc(tutorial ? "RuneboundTutorial" : "RuneboundEncounter");
		int encounterId = nextEncounterId++;
		NPC monolith = NPC.NewNPCDirect(source, position, ModContent.NPCType<BindingMonolith>());
		monolith.ai[0] = (float)family;
		monolith.ai[1] = (float)grade;
		monolith.ai[2] = encounterId;
		monolith.netUpdate = true;

		var encounter = new EncounterData
		{
			Id = encounterId,
			MonolithIndex = monolith.whoAmI,
			Family = family,
			Grade = grade,
			Tutorial = tutorial,
			MapEncounter = mapEncounter,
		};

		Encounters[encounterId] = encounter;
		SpawnBoundEnemy(source, encounter, ChooseCaptiveType(target), position + new Vector2(0f, -96f), themed: true);

		int guardianCount = tutorial ? 3 : Main.rand.Next(3, 6);
		for (int i = 0; i < guardianCount; i++)
		{
			float angle = MathHelper.TwoPi * i / guardianCount;
			Vector2 offset = new(MathF.Cos(angle) * 96f, -40f + MathF.Sin(angle) * 28f);
			SpawnBoundEnemy(source, encounter, ChooseGuardianType(target), position + offset, themed: false);
		}

		if (tutorial)
		{
			NotificationUtils.ShowNotification($"Mods.{PoTMod.ModName}.Misc.Runebound.TutorialSpawn", GetFamilyColor(family));
		}

		return true;
	}

	private static bool TryFindMapSpawnPosition(out Vector2 position)
	{
		var placement = new SpawnPlacement
		{
			Area = new Rectangle(40, 40, Main.maxTilesX - 80, Main.maxTilesY - 80),
			CollisionSize = new Point(12 * 16, 10 * 16),
			OnGround = true,
			MinDistanceFromPlayers = 500f,
			MaxDistanceFromPlayers = 0f,
			MinDistanceFromEnemies = 1200f,
			MaxDistanceFromEnemies = 0f,
			MaxSearchAttempts = 4096,
			SkippedLiquids = LiquidMask.All,
		};

		return EnemySpawning.TryFindingSpawnPosition(out position, in placement);
	}

	private static void SpawnBoundEnemy(IEntitySource source, EncounterData encounter, int npcType, Vector2 position, bool themed)
	{
		if (themed)
		{
			RuneboundSpawnContext.Prepare(encounter.Family);
			if (ScarabSystem.FindFamily(ScarabFamily.Binding) is { } binding
				&& (binding.Kind == ScarabKind.RunicMenagerie || binding.Grade == ScarabGrade.Prismatic))
			{
				ScarabSpawnContext.Prepare(1, 1.25f, 1.10f);
			}
		}

		NPC enemy;
		try
		{
			enemy = NPC.NewNPCDirect(source, position, npcType);
		}
		finally
		{
			RuneboundSpawnContext.Clear();
			ScarabSpawnContext.Clear();
		}

		if (!enemy.active)
		{
			return;
		}

		enemy.GetGlobalNPC<RuneboundNPC>().Bind(enemy, encounter.Id, encounter.Family, encounter.Grade);
		encounter.EnemyIndices.Add(enemy.whoAmI);
	}

	private static bool TryFindSpawnPosition(Player player, out Vector2 position)
	{
		Point origin = player.Center.ToTileCoordinates();

		for (int attempt = 0; attempt < 80; attempt++)
		{
			int direction = Main.rand.NextBool() ? 1 : -1;
			int x = origin.X + direction * Main.rand.Next(28, 50);
			int startY = Math.Clamp(origin.Y - 20, 20, Main.maxTilesY - 30);
			int endY = Math.Clamp(origin.Y + 45, 20, Main.maxTilesY - 20);

			if (!WorldGen.InWorld(x, startY, 10))
			{
				continue;
			}

			for (int y = startY; y <= endY; y++)
			{
				if (!WorldGen.SolidTile(x, y) || WorldGen.SolidTile(x, y - 1) || WorldGen.SolidTile(x, y - 2)
					|| WorldGen.SolidTile(x - 1, y - 1) || WorldGen.SolidTile(x + 1, y - 1))
				{
					continue;
				}

				position = new Vector2(x * 16f + 8f, y * 16f - 8f);
				return true;
			}
		}

		position = default;
		return false;
	}

	private static int ChooseCaptiveType(Player player)
	{
		if (player.ZoneSnow)
		{
			return NPCID.IceTortoise;
		}

		if (player.ZoneJungle)
		{
			return NPCID.AngryTrapper;
		}

		if (player.ZoneDesert)
		{
			return NPCID.Mummy;
		}

		return NPCID.ArmoredSkeleton;
	}

	private static int ChooseGuardianType(Player player)
	{
		if (player.ZoneSnow)
		{
			return NPCID.IceElemental;
		}

		if (player.ZoneJungle)
		{
			return NPCID.GiantTortoise;
		}

		if (player.ZoneDesert)
		{
			return NPCID.DarkMummy;
		}

		return NPCID.SkeletonArcher;
	}

	internal static void ActivateEncounter(int encounterId, Player activator)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || !Encounters.TryGetValue(encounterId, out EncounterData encounter)
			|| encounter.Active || !activator.active)
		{
			return;
		}

		encounter.Active = true;
		Vector2 center = Main.npc[encounter.MonolithIndex].Center;

		foreach (Player player in Main.ActivePlayers)
		{
			if (!player.dead && player.DistanceSQ(center) <= ParticipationRange * ParticipationRange)
			{
				encounter.Participants.Add(player.whoAmI);
			}
		}

		encounter.Participants.Add(activator.whoAmI);

		foreach (int npcIndex in encounter.EnemyIndices)
		{
			if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
			{
				Main.npc[npcIndex].GetGlobalNPC<RuneboundNPC>().Release(Main.npc[npcIndex]);
			}
		}

		if (encounter.MonolithIndex >= 0 && encounter.MonolithIndex < Main.maxNPCs)
		{
			Main.npc[encounter.MonolithIndex].active = false;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, encounter.MonolithIndex);
		}
	}

	internal static void NotifyEnemyKilled(int encounterId, int npcIndex)
	{
		if (!Encounters.TryGetValue(encounterId, out EncounterData encounter) || !encounter.Active)
		{
			return;
		}

		encounter.EnemyIndices.Remove(npcIndex);

		if (encounter.EnemyIndices.Any(index => index >= 0 && index < Main.maxNPCs && Main.npc[index].active))
		{
			return;
		}

		CompleteEncounter(encounter);
		Encounters.Remove(encounterId);
	}

	private static void CompleteEncounter(EncounterData encounter)
	{
		int rewardType = RollRewardType(encounter);
		Rectangle rewardArea = encounter.MonolithIndex >= 0 && encounter.MonolithIndex < Main.maxNPCs
			? Main.npc[encounter.MonolithIndex].Hitbox
			: new Rectangle(Main.spawnTileX * 16, Main.spawnTileY * 16, 16, 16);
		Vector2 rewardCenter = rewardArea.Center.ToVector2();

		// A player who joined the fight after activation still participates if they are present for the clear.
		foreach (Player nearbyPlayer in Main.ActivePlayers)
		{
			if (!nearbyPlayer.dead && nearbyPlayer.DistanceSQ(rewardCenter) <= ParticipationRange * ParticipationRange)
			{
				encounter.Participants.Add(nearbyPlayer.whoAmI);
			}
		}

		foreach (int participantIndex in encounter.Participants)
		{
			if (participantIndex < 0 || participantIndex >= Main.maxPlayers || !Main.player[participantIndex].active)
			{
				continue;
			}

			Player player = Main.player[participantIndex];
			DropInstancedReward(player, rewardArea, rewardType);
			RuneboundPlayer progress = player.GetModPlayer<RuneboundPlayer>();
			progress.EncountersCompleted++;

			if (encounter.Tutorial && progress.TutorialEncounterRequested)
			{
				progress.TutorialEncounterRequested = false;
				progress.TutorialEncounterCompleted = true;
			}

			if (Main.netMode == NetmodeID.Server)
			{
				RuneboundProgressHandler.Send(player);
			}
		}

		if (encounter.MapEncounter)
		{
			mapEncountersCompleted++;
			if (ScarabSystem.Has(ScarabKind.RunicMenagerie))
			{
				int familyIndex = encounter.Family switch
				{
					RuneboundFamily.Vigor => 0,
					RuneboundFamily.Embers => 1,
					RuneboundFamily.Void => 2,
					_ => -1,
				};
				if (familyIndex >= 0)
				{
					menagerieCompletedFamilyMask |= 1 << familyIndex;
				}
			}
		}
	}

	private static void DropInstancedReward(Player player, Rectangle area, int itemType)
	{
		var source = new EntitySource_Misc("RuneboundReward");

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			Item.NewItem(source, area, itemType);
			return;
		}

		int itemIndex = Item.NewItem(source, area, itemType, noBroadcast: true);
		Main.timeItemSlotCannotBeReusedFor[itemIndex] = 54000;
		NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, -1, null, itemIndex);
		Main.item[itemIndex].active = false;
	}

	private static RuneboundFamily RollFamily()
	{
		if (IsExplorationMap() && ScarabSystem.Has(ScarabKind.RunicMenagerie))
		{
			RuneboundFamily[] menagerie = [RuneboundFamily.Vigor, RuneboundFamily.Embers, RuneboundFamily.Void];
			for (int i = 0; i < menagerie.Length; i++)
			{
				RuneboundFamily family = menagerie[i];
				bool completed = (menagerieCompletedFamilyMask & (1 << i)) != 0;
				bool alreadyLoaded = Encounters.Values.Any(encounter => encounter.MapEncounter && encounter.Family == family);
				if (!completed && !alreadyLoaded)
				{
					return family;
				}
			}
		}

		return (RuneboundFamily)Main.rand.Next(Enum.GetValues<RuneboundFamily>().Length);
	}

	private static RunestoneGrade RollGrade()
	{
		int areaLevel = SubworldSystem.Current is MappingWorld ? MappingWorld.AreaLevel : 50;
		int roll = Main.rand.Next(1000);

		if (areaLevel >= 70 && roll < 35)
		{
			return RunestoneGrade.Perfect;
		}

		return areaLevel >= 55 && roll < 250 ? RunestoneGrade.Greater : RunestoneGrade.Faint;
	}

	private static int RollRewardType(EncounterData encounter)
	{
		int areaLevel = SubworldSystem.Current is MappingWorld ? MappingWorld.AreaLevel : 50;
		bool chaseEligible = !encounter.Tutorial && (areaLevel >= 70 || NPC.downedMoonlord);
		int chaseChance = areaLevel >= 80 ? 30 : 12;

		if (chaseEligible && Main.rand.Next(1000) < chaseChance)
		{
			return Main.rand.Next(4) switch
			{
				0 => ModContent.ItemType<ForbiddenRunestoneOfThePrism>(),
				1 => ModContent.ItemType<ForbiddenRunestoneOfTheTitan>(),
				2 => ModContent.ItemType<ForbiddenRunestoneOfAnnihilation>(),
				_ => ModContent.ItemType<ForbiddenRunestoneOfTrinity>(),
			};
		}

		RunestoneGrade rewardGrade = encounter.Grade;
		if (ScarabSystem.FindFamily(ScarabFamily.Binding) is { } binding)
		{
			int upgradeChance = binding.Kind == ScarabKind.RunicMenagerie ? 25 : binding.Grade switch
			{
				ScarabGrade.Gilded => 15,
				ScarabGrade.Prismatic => 30,
				_ => 0,
			};
			if (binding.Kind == ScarabKind.RunicMenagerie && Main.rand.Next(100) < upgradeChance)
			{
				rewardGrade = RunestoneGrade.Perfect;
			}
			else if (Main.rand.Next(100) < upgradeChance)
			{
				rewardGrade = (RunestoneGrade)Math.Min((int)RunestoneGrade.Perfect, (int)rewardGrade + 1);
			}
		}

		return GetRunestoneType(encounter.Family, rewardGrade);
	}

	internal static Color GetFamilyColor(RuneboundFamily family)
	{
		return family switch
		{
			RuneboundFamily.Vigor => new Color(204, 62, 84),
			RuneboundFamily.Bastion => new Color(176, 164, 139),
			RuneboundFamily.Embers => new Color(242, 99, 40),
			RuneboundFamily.Rime => new Color(91, 189, 225),
			RuneboundFamily.Tempests => new Color(246, 216, 73),
			RuneboundFamily.Void => new Color(143, 73, 204),
			RuneboundFamily.Might => new Color(205, 58, 45),
			RuneboundFamily.Precision => new Color(226, 189, 75),
			RuneboundFamily.Haste => new Color(67, 207, 145),
			RuneboundFamily.Spirit => new Color(86, 120, 224),
			_ => Color.White,
		};
	}

	private static int GetRunestoneType(RuneboundFamily family, RunestoneGrade grade)
	{
		return (family, grade) switch
		{
			(RuneboundFamily.Vigor, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfVigor>(),
			(RuneboundFamily.Vigor, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfVigor>(),
			(RuneboundFamily.Vigor, _) => ModContent.ItemType<PerfectRunestoneOfVigor>(),
			(RuneboundFamily.Bastion, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfTheBastion>(),
			(RuneboundFamily.Bastion, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfTheBastion>(),
			(RuneboundFamily.Bastion, _) => ModContent.ItemType<PerfectRunestoneOfTheBastion>(),
			(RuneboundFamily.Embers, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfEmbers>(),
			(RuneboundFamily.Embers, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfEmbers>(),
			(RuneboundFamily.Embers, _) => ModContent.ItemType<PerfectRunestoneOfEmbers>(),
			(RuneboundFamily.Rime, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfRime>(),
			(RuneboundFamily.Rime, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfRime>(),
			(RuneboundFamily.Rime, _) => ModContent.ItemType<PerfectRunestoneOfRime>(),
			(RuneboundFamily.Tempests, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfTempests>(),
			(RuneboundFamily.Tempests, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfTempests>(),
			(RuneboundFamily.Tempests, _) => ModContent.ItemType<PerfectRunestoneOfTempests>(),
			(RuneboundFamily.Void, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfTheVoid>(),
			(RuneboundFamily.Void, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfTheVoid>(),
			(RuneboundFamily.Void, _) => ModContent.ItemType<PerfectRunestoneOfTheVoid>(),
			(RuneboundFamily.Might, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfMight>(),
			(RuneboundFamily.Might, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfMight>(),
			(RuneboundFamily.Might, _) => ModContent.ItemType<PerfectRunestoneOfMight>(),
			(RuneboundFamily.Precision, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfPrecision>(),
			(RuneboundFamily.Precision, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfPrecision>(),
			(RuneboundFamily.Precision, _) => ModContent.ItemType<PerfectRunestoneOfPrecision>(),
			(RuneboundFamily.Haste, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfHaste>(),
			(RuneboundFamily.Haste, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfHaste>(),
			(RuneboundFamily.Haste, _) => ModContent.ItemType<PerfectRunestoneOfHaste>(),
			(RuneboundFamily.Spirit, RunestoneGrade.Faint) => ModContent.ItemType<FaintRunestoneOfSpirit>(),
			(RuneboundFamily.Spirit, RunestoneGrade.Greater) => ModContent.ItemType<GreaterRunestoneOfSpirit>(),
			_ => ModContent.ItemType<PerfectRunestoneOfSpirit>(),
		};
	}
}
