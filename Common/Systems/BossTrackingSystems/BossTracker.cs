using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PathOfTerraria.Common.NPCs.GlobalNPCs;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Content.Swamp.NPCs.SwampBoss;
using PathOfTerraria.Utilities.Terraria;
using SubworldLibraryCommunityFork;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.BossTrackingSystems;

internal sealed class BossTracker : ModSystem
{
	/// <summary>
	/// Tracks the bosses that were killed in a <see cref="BossDomainSubworld"/>, so we can delay the effects to the main world. 
	/// This can also be used for non-universal kills per subworld, i.e.<br/>
	/// <c>if (SubworldSystem.Current is BossDomainSubworld &amp;&amp; CachedBossesDowned.Contains(id)</c><br/>
	/// you know you are in a subworld where a boss was slain.
	/// </summary>
	public static HashSet<int> CachedBossesDowned = [];

	/// <summary>
	/// Tracks all bosses that have ever been downed. If they're in this set, they've been killed at least once before by any player.
	/// </summary>
	public static HashSet<int> TotalBossesDowned = [];

	private const int StableBossIdFormatVersion = 1;
	private const string FormatVersionKey = "StableBossIdFormatVersion";
	private const string VanillaIdsSuffix = "VanillaIds";
	private const string ModdedNamesSuffix = "ModdedNames";
	private const string UnresolvedLegacyIdsSuffix = "UnresolvedLegacyIds";
	private const string LegacyBackupIdsSuffix = "LegacyBackupIds";

	private static readonly HashSet<string> UnresolvedCachedBossNames = [];
	private static readonly HashSet<string> UnresolvedTotalBossNames = [];
	private static readonly HashSet<int> UnresolvedCachedLegacyIds = [];
	private static readonly HashSet<int> UnresolvedTotalLegacyIds = [];
	private static readonly HashSet<int> CachedLegacyBackupIds = [];
	private static readonly HashSet<int> TotalLegacyBackupIds = [];

	private static bool _legacyRecoveryWarningPending;

	public static bool SkipWoFBox;
	public static bool AlwaysSkipWoFBox = true;

	private static readonly MethodInfo DoDeathEventsInfo = typeof(NPC).GetMethod("DoDeathEvents", BindingFlags.Instance | BindingFlags.NonPublic);

	public override void Load()
	{
		On_NPC.DoDeathEvents += HijackDeathEffects;
		On_NPC.CreateBrickBoxForWallOfFlesh += StopBrickBox;
	}

	public override void ClearWorld()
	{
		ClearTrackedData();
	}

	public override void PostUpdateWorld()
	{
		if (!_legacyRecoveryWarningPending || Main.netMode == NetmodeID.Server || Main.gameMenu || !Main.LocalPlayer.active)
		{
			return;
		}

		_legacyRecoveryWarningPending = false;
		Main.NewText(Language.GetTextValue("Mods.PathOfTerraria.Misc.BossTrackerLegacyRecoveryWarning"), Color.OrangeRed);
	}

	public static void AddDowned(int id, bool fromSync = false, bool setBossDowned = false, bool dontCache = false)
	{
		if (!dontCache)
		{
			CachedBossesDowned.Add(id);
		}
		
		TotalBossesDowned.Add(id);

		if (Main.netMode != NetmodeID.SinglePlayer && !fromSync)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.RequestWorldData, -1, -1, null, Main.myPlayer);
			}
			else
			{
				NetMessage.SendData(MessageID.WorldData);
			}

			if (SubworldSystem.Current is not null && setBossDowned)
			{
				ModPacket packet = Networking.GetPacket<SyncBossDownedHandler>(5);
				packet.Write(id);
				Networking.SendPacketToMainServer(packet);
			}
		}
	}

	public static bool DownedInDomain<T>(int id) where T : BossDomainSubworld
	{
		return CachedBossesDowned.Contains(id) && SubworldSystem.Current is T;
	}

	public static bool DownedInDomain<T>(params int[] ids) where T : BossDomainSubworld
	{
		if (SubworldSystem.Current is not T)
		{
			return false;
		}

		foreach (int id in ids)
		{
			if (!CachedBossesDowned.Contains(id))
			{
				return false;
			}
		}

		return true;
	}

	private static void StopBrickBox(On_NPC.orig_CreateBrickBoxForWallOfFlesh orig, NPC self)
	{
		if (!SkipWoFBox && !AlwaysSkipWoFBox)
		{
			orig(self);
		}
	}

	private static void HijackDeathEffects(On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer)
	{
		bool isBoss = ContentSamples.NpcsByNetId[self.netID].boss || NPCID.Sets.ShouldBeCountedAsBoss[self.type];
		bool isPillar = self.type is NPCID.LunarTowerNebula or NPCID.LunarTowerSolar or NPCID.LunarTowerStardust or NPCID.LunarTowerVortex;
		int type = self.type;
		bool oldBoss = self.boss;

		if (SubworldSystem.Current is BossDomainSubworld or IExplorationWorld && isBoss && !isPillar)
		{
			// Spawns the Wall of Flesh's box around itself, which is overriden by this method
			OnDeathNPC.OnDeathEffects(self);

			if (CheckSpecialConditions(self, isBoss))
			{
				// Automatically add/send the boss downed cache/packet
				AddDowned(self.netID, false, true, false);

				// Sends both bosses for consistency for the Twins
				if (self.netID == NPCID.Retinazer)
				{
					AddDowned(NPCID.Spazmatism, false, true, false);
				}
				else if (self.netID == NPCID.Spazmatism)
				{
					AddDowned(NPCID.Retinazer, false, true, false);
				}
			}

			self.type = NPCID.None;
			self.boss = false;
		}

		orig(self, closestPlayer);
	}

	private static bool CheckSpecialConditions(NPC self, bool isBoss)
	{
		int type = self.type;

		if (NPCUtil.IsEaterOfWorldsPart(type)) // EoW should only count once every other EoW segment is dead
		{
			return NPCUtil.IsLastEaterOfWorldsPart(self);
		}

		// Only count as downed when there's 1 mossmother left
		if (type == ModContent.NPCType<Mossmother>() && NPC.CountNPCS(ModContent.NPCType<Mossmother>()) > 1)
		{
			return false;
		}

		if (!isBoss)
		{
			return false;
		}

		if (type == NPCID.Spazmatism) // Spazmatism/Retinazer only count if the other is defeated
		{
			return !NPC.AnyNPCs(NPCID.Retinazer);
		}
		else if (type == NPCID.Retinazer)
		{
			return !NPC.AnyNPCs(NPCID.Spazmatism);
		}

		return isBoss;
	}

	public override void PreUpdateEntities()
	{
		if (SubworldSystem.Current is null && CachedBossesDowned.Count > 0)
		{
			foreach (int type in CachedBossesDowned)
			{
				if (TryApplyCachedBossWithoutDeathReplay(type))
				{
					continue;
				}

				var npc = new NPC();
				npc.SetDefaults(type);
				npc.Center = Main.player[0].Center;

				if (npc.type == NPCID.EaterofWorldsHead)
				{
					npc.boss = true;
				}

				try
				{
					SkipWoFBox = true;
					DoDeathEventsInfo.Invoke(npc, [Main.player[0]]);
				}
				finally
				{
					SkipWoFBox = false;
				}
			}

			CachedBossesDowned.Clear();
		}
	}

	private static bool TryApplyCachedBossWithoutDeathReplay(int type)
	{
		switch (type)
		{
			case NPCID.QueenBee:
				// Queen Bee sets this flag during loot, before DoDeathEvents; replaying a synthetic death can crash on SP return.
				NPC.SetEventFlagCleared(ref NPC.downedQueenBee, GameEventClearedID.DefeatedQueenBee);

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
				}

				return true;
		}

		return false;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		WritePersistentData(tag, nameof(CachedBossesDowned), nameof(TotalBossesDowned));
	}
	public override void LoadWorldData(TagCompound tag)
	{
		ClearTrackedData();
		ReadPersistentData(tag, nameof(CachedBossesDowned), nameof(TotalBossesDowned));

		// Load legacy data.
		if (tag.ContainsKey("DownedFlags"))
		{
			BitsByte oldMask = tag.GetByte("DownedFlags");

			// These calls are counted as "fromSync" since they don't need to sync - incoming players will have the result synced for them, and
			// at the time this is loaded players can't be joined to the server already
			if (oldMask[0]) { EventTracker.CompleteEvent(EventFlags.DefeatedEaterOfWorlds, fromSync: true); }
			if (oldMask[1]) { EventTracker.CompleteEvent(EventFlags.DefeatedBrainOfCthulhu, fromSync: true); }
		}

		if (HasUnresolvedBossHistory)
		{
			PoTMod.Instance.Logger.Warn("Some legacy modded boss history could not be identified. The raw ids were preserved for recovery with the previous mod/content layout.");
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(CachedBossesDowned.Count);
		foreach (int item in CachedBossesDowned)
		{
			writer.Write(item);
		}

		writer.Write(TotalBossesDowned.Count);
		foreach (int item in TotalBossesDowned)
		{
			writer.Write(item);
		}

		writer.Write(HasUnresolvedBossHistory);
	}
	public override void NetReceive(BinaryReader reader)
	{
		ClearTrackedData();

		for (int i = 0, count = reader.ReadInt32(); i < count; ++i)
		{
			CachedBossesDowned.Add(reader.ReadInt32());
		}

		for (int i = 0, count = reader.ReadInt32(); i < count; ++i)
		{
			TotalBossesDowned.Add(reader.ReadInt32());
		}

		_legacyRecoveryWarningPending = reader.ReadBoolean();
	}

	public static void WriteConsistentInfo(TagCompound tag)
	{
		WritePersistentData(tag, "cached", "total");
	}
	public static void ReadConsistentInfo(TagCompound tag)
	{
		ClearTrackedData();
		ReadPersistentData(tag, "cached", "total");
	}

	private static bool HasUnresolvedBossHistory => UnresolvedCachedBossNames.Count > 0
		|| UnresolvedTotalBossNames.Count > 0
		|| UnresolvedCachedLegacyIds.Count > 0
		|| UnresolvedTotalLegacyIds.Count > 0;

	private static void WritePersistentData(TagCompound tag, string cachedKey, string totalKey)
	{
		tag[FormatVersionKey] = StableBossIdFormatVersion;
		WriteBossSet(tag, cachedKey, CachedBossesDowned, UnresolvedCachedBossNames, UnresolvedCachedLegacyIds, CachedLegacyBackupIds);
		WriteBossSet(tag, totalKey, TotalBossesDowned, UnresolvedTotalBossNames, UnresolvedTotalLegacyIds, TotalLegacyBackupIds);
	}

	private static void WriteBossSet(TagCompound tag, string key, HashSet<int> bosses, HashSet<string> unresolvedNames,
		HashSet<int> unresolvedLegacyIds, HashSet<int> legacyBackupIds)
	{
		List<int> vanillaIds = [];
		HashSet<string> moddedNames = [.. unresolvedNames];
		HashSet<int> compatibilityIds = [.. unresolvedLegacyIds];

		foreach (int id in bosses)
		{
			compatibilityIds.Add(id);

			if (IsVanillaNpcId(id))
			{
				vanillaIds.Add(id);
			}
			else if (TryGetModdedNpcName(id, out string name))
			{
				moddedNames.Add(name);
			}
			else
			{
				compatibilityIds.Add(id);
			}
		}

		// Kept for one-version rollback compatibility. Current code ignores this field once FormatVersionKey is present.
		tag[key] = (int[])[.. compatibilityIds];
		tag[key + VanillaIdsSuffix] = (int[])[.. vanillaIds];
		tag[key + ModdedNamesSuffix] = new List<string>(moddedNames);

		if (unresolvedLegacyIds.Count > 0)
		{
			tag[key + UnresolvedLegacyIdsSuffix] = (int[])[.. unresolvedLegacyIds];
		}

		if (legacyBackupIds.Count > 0)
		{
			tag[key + LegacyBackupIdsSuffix] = (int[])[.. legacyBackupIds];
		}
	}

	private static void ReadPersistentData(TagCompound tag, string cachedKey, string totalKey)
	{
		ReadStableBossSet(tag, cachedKey, CachedBossesDowned, UnresolvedCachedBossNames, UnresolvedCachedLegacyIds,
			CachedLegacyBackupIds);
		ReadStableBossSet(tag, totalKey, TotalBossesDowned, UnresolvedTotalBossNames, UnresolvedTotalLegacyIds,
			TotalLegacyBackupIds);

		if (tag.GetInt(FormatVersionKey) < StableBossIdFormatVersion)
		{
			MigrateLegacyBossSets(tag.GetIntArray(cachedKey), tag.GetIntArray(totalKey));
		}

		_legacyRecoveryWarningPending = HasUnresolvedBossHistory;
	}

	private static void ReadStableBossSet(TagCompound tag, string key, HashSet<int> bosses, HashSet<string> unresolvedNames,
		HashSet<int> unresolvedLegacyIds, HashSet<int> legacyBackupIds)
	{
		foreach (int id in tag.GetIntArray(key + VanillaIdsSuffix))
		{
			if (IsVanillaNpcId(id))
			{
				bosses.Add(id);
			}
		}

		foreach (string name in tag.GetList<string>(key + ModdedNamesSuffix))
		{
			if (ModContent.TryFind(name, out ModNPC npc))
			{
				bosses.Add(npc.Type);
			}
			else
			{
				unresolvedNames.Add(name);
			}
		}

		unresolvedLegacyIds.UnionWith(tag.GetIntArray(key + UnresolvedLegacyIdsSuffix));
		legacyBackupIds.UnionWith(tag.GetIntArray(key + LegacyBackupIdsSuffix));
	}

	private static void MigrateLegacyBossSets(int[] cachedIds, int[] totalIds)
	{
		HashSet<int> moddedIds = [];

		AddVanillaAndCollectModded(cachedIds, CachedBossesDowned, moddedIds);
		AddVanillaAndCollectModded(totalIds, TotalBossesDowned, moddedIds);

		if (moddedIds.Count == 0)
		{
			return;
		}

		int offset = 0;
		bool canMigrate = LegacyBossIdsMatchOffset(moddedIds, offset) || TryFindUniqueLegacyBossOffset(moddedIds, out offset);

		MigrateLegacyBossSet(cachedIds, CachedBossesDowned, UnresolvedCachedLegacyIds, CachedLegacyBackupIds, canMigrate, offset);
		MigrateLegacyBossSet(totalIds, TotalBossesDowned, UnresolvedTotalLegacyIds, TotalLegacyBackupIds, canMigrate, offset);
	}

	private static void AddVanillaAndCollectModded(IEnumerable<int> ids, HashSet<int> bosses, HashSet<int> moddedIds)
	{
		foreach (int id in ids)
		{
			if (IsVanillaNpcId(id))
			{
				bosses.Add(id);
			}
			else
			{
				moddedIds.Add(id);
			}
		}
	}

	private static void MigrateLegacyBossSet(IEnumerable<int> ids, HashSet<int> bosses, HashSet<int> unresolvedLegacyIds,
		HashSet<int> legacyBackupIds, bool canMigrate, int offset)
	{
		foreach (int id in ids)
		{
			if (IsVanillaNpcId(id))
			{
				continue;
			}

			if (canMigrate)
			{
				bosses.Add(id + offset);
				legacyBackupIds.Add(id);
			}
			else
			{
				unresolvedLegacyIds.Add(id);
			}
		}
	}

	private static bool TryFindUniqueLegacyBossOffset(HashSet<int> legacyIds, out int offset)
	{
		offset = 0;
		bool found = false;
		int firstId = 0;

		foreach (int id in legacyIds)
		{
			firstId = id;
			break;
		}

		foreach (ModNPC npc in ModContent.GetContent<ModNPC>())
		{
			if (!IsPathOfTerrariaBoss(npc.Type))
			{
				continue;
			}

			long candidateValue = (long)npc.Type - firstId;

			if (candidateValue is < int.MinValue or > int.MaxValue)
			{
				continue;
			}

			int candidate = (int)candidateValue;

			if (!LegacyBossIdsMatchOffset(legacyIds, candidate))
			{
				continue;
			}

			if (found)
			{
				return false;
			}

			found = true;
			offset = candidate;
		}

		return found;
	}

	private static bool LegacyBossIdsMatchOffset(IEnumerable<int> legacyIds, int offset)
	{
		foreach (int id in legacyIds)
		{
			long mappedType = (long)id + offset;

			if (mappedType < NPCID.Count || mappedType > int.MaxValue || !IsPathOfTerrariaBoss((int)mappedType))
			{
				return false;
			}
		}

		return true;
	}

	private static bool IsPathOfTerrariaBoss(int type)
	{
		return NPCLoader.GetNPC(type) is { Mod: PoTMod }
			&& ContentSamples.NpcsByNetId.TryGetValue(type, out NPC sample)
			&& (sample.boss || NPCID.Sets.ShouldBeCountedAsBoss[type]);
	}

	private static bool IsVanillaNpcId(int id)
	{
		return id < NPCID.Count;
	}

	private static bool TryGetModdedNpcName(int type, out string name)
	{
		name = null;

		if (type < NPCID.Count || type >= NPCLoader.NPCCount || NPCLoader.GetNPC(type) is not ModNPC npc)
		{
			return false;
		}

		name = npc.FullName;
		return true;
	}

	private static void ClearTrackedData()
	{
		CachedBossesDowned.Clear();
		TotalBossesDowned.Clear();
		UnresolvedCachedBossNames.Clear();
		UnresolvedTotalBossNames.Clear();
		UnresolvedCachedLegacyIds.Clear();
		UnresolvedTotalLegacyIds.Clear();
		CachedLegacyBackupIds.Clear();
		TotalLegacyBackupIds.Clear();
		_legacyRecoveryWarningPending = false;
	}

	public class BossTrackerNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.type == NPCID.EaterofWorldsHead && NPC.CountNPCS(NPCID.EaterofWorldsBody) == 0)
			{
				// EoW is dumb and won't register as a boss before death so this is the workaround
				EventTracker.CompleteEvent(EventFlags.DefeatedEaterOfWorlds, gameEventId: GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}

			if (!npc.boss)
			{
				return;
			}

			if (npc.type == NPCID.BrainofCthulhu)
			{
				EventTracker.CompleteEvent(EventFlags.DefeatedBrainOfCthulhu, gameEventId: GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
	}
}
