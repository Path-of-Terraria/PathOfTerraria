using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Synchronization;
using SubworldLibrary;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.BossTrackingSystems;

// It is okay to change and reorder values here, but do not rename them unless you edit save/load.
[Flags]
public enum EventFlags : ulong
{
	// Custom
	DefeatedEaterOfWorlds = 1ul << 0,
	DefeatedBrainOfCthulhu = 1ul << 1,
	// Invasions
	DefeatedGoblinArmy = 1ul << 2,
	DefeatedPirates = 1ul << 3,
	DefeatedMartians = 1ul << 4,
	DefeatedFrostLegion = 1ul << 5,
	// Misc.
	DefeatedClown = 1ul << 6,
	// Bosses
	DefeatedKingSlime = 1ul << 7,
	DefeatedEyeOfCthulhu = 1ul << 8,
	DefeatedEowOrBoc = 1ul << 9,
	DefeatedQueenBee = 1ul << 10,
	DefeatedSkeletron = 1ul << 11,
	DefeatedDeerclops = 1ul << 12,
	DefeatedQueenSlime = 1ul << 13,
	DefeatedMechBossAny = 1ul << 14,
	DefeatedDestroyer = 1ul << 15,
	DefeatedTheTwins = 1ul << 16,
	DefeatedSkeletronPrime = 1ul << 17,
	DefeatedPlantBoss = 1ul << 18,
	DefeatedEmpressOfLight = 1ul << 19,
	DefeatedFishron = 1ul << 20,
	DefeatedGolemBoss = 1ul << 21,
	DefeatedHalloweenTree = 1ul << 22,
	DefeatedHalloweenKing = 1ul << 23,
	DefeatedChristmasTree = 1ul << 24,
	DefeatedChristmasSantank = 1ul << 25,
	DefeatedChristmasIceQueen = 1ul << 26,
	DefeatedAncientCultist = 1ul << 27,
	DefeatedMoonlord = 1ul << 28,
	DefeatedTowerSolar = 1ul << 29,
	DefeatedTowerVortex = 1ul << 30,
	DefeatedTowerNebula = 1ul << 31,
	DefeatedTowerStardust = 1ul << 32,
	DefeatedOldOnesArmyT1 = 1ul << 33,
	DefeatedOldOnesArmyT2 = 1ul << 34,
	DefeatedOldOnesArmyT3 = 1ul << 35,
}

internal class SyncEventCompletionHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncEventCompletion;

	/// <inheritdoc cref="Networking.Message.SyncEventCompletion"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out EventFlags flags, out int? gameEventId);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write((ulong)flags);
		packet.Write(gameEventId ?? int.MaxValue);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		var flag = (EventFlags)reader.ReadUInt64();
		int? gameEventId = reader.ReadInt32() is not int.MaxValue and int idx ? idx : null;

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write((ulong)flag);
		packet.Write(gameEventId ?? int.MaxValue);
		packet.Send();

		EventTracker.CompleteEvent(flag, gameEventId, fromSync: true);
		PoTMod.Instance.Logger.Debug("Got EVENT: " + flag);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		var flag = (EventFlags)reader.ReadUInt64();
		int? gameEventId = reader.ReadInt32() is not int.MaxValue and int idx ? idx : null;

		EventTracker.CompleteEvent(flag, gameEventId, fromSync: true);
	}
}

/// <summary> This system is responsible for storing and distributing information about event completion. </summary>
internal sealed class EventTracker : ModSystem
{
	private static readonly EventFlags[] EventFlagsValues = Enum.GetValues<EventFlags>();
	private static readonly Dictionary<string, EventFlags> EventFlagLookup = EventFlagsValues.ToDictionary(f => Enum.GetName(f));

	/// <summary> New flags specific to the current world, which are to be carried over to the main world. </summary>
	private static readonly HashSet<(EventFlags Flags, int? GameEventId)> cachedEventCompletions = [];
	/// <summary> Flags ever set in any world. </summary>
	private static EventFlags globalFlags;
	/// <summary> Flags set in the local world. Both a storage for new flags and a performance convenience for vanilla ones. </summary>
	private static EventFlags localFlags;
	private static bool skipTrackingEventCompletions;

	public override void Load()
	{
		On_NPC.SetEventFlagCleared += OnSetEventFlagCleared;
	}

	public override void PreUpdateEntities()
	{
		// Set any cached event completion flags using vanilla code where possible, starting lantern night celebrations when fit, etc.
		if (SubworldSystem.Current is null && cachedEventCompletions.Count != 0)
		{
			skipTrackingEventCompletions = true;

			foreach ((EventFlags flag, int? gameEventId) in cachedEventCompletions)
			{
				ref bool valueRef = ref GetExternalFlagValueRefOrNull(flag);
			
				if (!Unsafe.IsNullRef(ref valueRef))
				{
					if (gameEventId.HasValue)
					{
						skipTrackingEventCompletions = true;
						NPC.SetEventFlagCleared(ref valueRef, gameEventId.Value);
						skipTrackingEventCompletions = false;
					}
					else
					{
						valueRef = true;
					}
				}
			}

			// Synchronize NPC.downedX values.
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}

			cachedEventCompletions.Clear();
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		ImportVanillaFlags();

		// Only save what is not already stored somewhere.
		tag.Add("LocalFlags", EventFlagsValues
			.Where(f => Unsafe.IsNullRef(ref GetExternalFlagValueRefOrNull(f)))
			.Where(f => (localFlags & f) == f)
			.Select(f => Enum.GetName(f))
			.ToArray()
		);
	}
	public override void LoadWorldData(TagCompound tag)
	{
		if (tag.TryGet("LocalFlags", out string[] localFlagNames))
		{
			localFlags = 0;

			foreach (string flagName in localFlagNames)
			{
				localFlags |= EventFlagLookup.TryGetValue(flagName, out EventFlags flag) ? flag : 0;
			}
		}
		else if (tag.ContainsKey("DownedFlags"))
		{
			// Load legacy data.
			BitsByte oldMask = tag.GetByte("DownedFlags");
			localFlags = 0;
			localFlags |= oldMask[0] ? EventFlags.DefeatedEaterOfWorlds : 0;
			localFlags |= oldMask[1] ? EventFlags.DefeatedBrainOfCthulhu : 0;
		}

		ImportVanillaFlags();
		globalFlags |= localFlags;
	}

	public override void NetSend(BinaryWriter writer)
	{
		ImportVanillaFlags();
		writer.Write((ulong)globalFlags);
		writer.Write((ulong)localFlags);
		writer.Write7BitEncodedInt(cachedEventCompletions.Count);
		foreach ((EventFlags flag, int? eventId) in cachedEventCompletions)
		{
			writer.Write((ulong)flag);
			writer.Write(eventId ?? int.MaxValue);
		}
	}
	public override void NetReceive(BinaryReader reader)
	{
		globalFlags = (EventFlags)reader.ReadUInt64();
		localFlags = (EventFlags)reader.ReadUInt64();
		for (int i = 0, count = reader.Read7BitEncodedInt(); i < count; ++i)
		{
			cachedEventCompletions.Add(((EventFlags)reader.ReadUInt64(), reader.ReadInt32() is not int.MaxValue and int idx ? idx : null));
		}
	}

	public static void WriteConsistentInfo(TagCompound tag)
	{
		tag.Add(nameof(globalFlags), (ulong)globalFlags);

		tag.Add($"{nameof(cachedEventCompletions)}Flags", cachedEventCompletions.Select(t => (ulong)t.Flags).ToArray());
		tag.Add($"{nameof(cachedEventCompletions)}Events", cachedEventCompletions.Select(t => t.GameEventId ?? int.MaxValue).ToArray());
	}
	public static void ReadConsistentInfo(TagCompound tag)
	{
		globalFlags |= unchecked((EventFlags)tag.Get<long>(nameof(globalFlags)));

		ulong[] completionFlags = tag.Get<ulong[]>($"{nameof(cachedEventCompletions)}Flags");
		int[] completionEvents = tag.Get<int[]>($"{nameof(cachedEventCompletions)}Events");

		for (int i = 0; i < completionFlags.Length; i++)
		{
			cachedEventCompletions.Add(((EventFlags)completionFlags[i], completionEvents[i] is not int.MaxValue and int idx ? idx : null));
		}
	}

	public static bool HasFlagsAnywhere(EventFlags flags)
	{
		ImportVanillaFlags();

		return (globalFlags & flags) == flags;
	}
	public static bool HasFlagsInDomain<T>(EventFlags flags) where T : BossDomainSubworld
	{
		ImportVanillaFlags();

		return SubworldSystem.Current is T && (localFlags & flags) == flags;
	}

	public static void CompleteEvent(EventFlags singleEvent, int? gameEventId = null, bool fromSync = false)
	{
		// Expect single values, not masks.
		Debug.Assert(BitOperations.PopCount((ulong)singleEvent) == 1);

		localFlags |= singleEvent;
		globalFlags |= singleEvent;
		cachedEventCompletions.Add((singleEvent, gameEventId));

		if (Main.netMode == NetmodeID.Server && !fromSync)
		{
			ModPacket packet = Networking.GetPacket(Networking.Message.SyncEventCompletion, capacity: 12);
			packet.Write((ulong)singleEvent);
			packet.Write(gameEventId ?? int.MaxValue);
			Networking.SendPacketToMainServer(packet);
		}
	}

	/// <summary> Updates local & global flags using the world's NPC.downedX values. Local flags are reset, while global flags are added to. </summary>
	public static void ImportVanillaFlags()
	{
		foreach (EventFlags flag in EventFlagsValues)
		{
			ref bool valueRef = ref GetExternalFlagValueRefOrNull(flag);

			if (!Unsafe.IsNullRef(ref valueRef))
			{
				localFlags = valueRef ? (localFlags | flag) : (localFlags & ~flag);
				globalFlags = valueRef ? (globalFlags | flag) : globalFlags;
			}
		}
	}

	private static ref bool GetExternalFlagValueRefOrNull(EventFlags flag)
	{
		// Expect single values, not masks.
		Debug.Assert(BitOperations.PopCount((ulong)flag) == 1);

		switch (flag)
		{
			// Customs
			case EventFlags.DefeatedBrainOfCthulhu:
			case EventFlags.DefeatedEaterOfWorlds:
				return ref Unsafe.NullRef<bool>();
			// Vanilla
			case EventFlags.DefeatedGoblinArmy: return ref NPC.downedGoblins;
			case EventFlags.DefeatedPirates: return ref NPC.downedPirates;
			case EventFlags.DefeatedMartians: return ref NPC.downedMartians;
			case EventFlags.DefeatedFrostLegion: return ref NPC.downedFrost;
			case EventFlags.DefeatedClown: return ref NPC.downedClown;
			case EventFlags.DefeatedKingSlime: return ref NPC.downedSlimeKing;
			case EventFlags.DefeatedEyeOfCthulhu: return ref NPC.downedBoss1;
			case EventFlags.DefeatedEowOrBoc: return ref NPC.downedBoss2;
			case EventFlags.DefeatedSkeletron: return ref NPC.downedBoss3;
			case EventFlags.DefeatedQueenBee: return ref NPC.downedQueenBee;
			case EventFlags.DefeatedDeerclops: return ref NPC.downedDeerclops;
			case EventFlags.DefeatedQueenSlime: return ref NPC.downedQueenSlime;
			case EventFlags.DefeatedMechBossAny: return ref NPC.downedMechBossAny;
			case EventFlags.DefeatedDestroyer: return ref NPC.downedMechBoss1;
			case EventFlags.DefeatedTheTwins: return ref NPC.downedMechBoss2;
			case EventFlags.DefeatedSkeletronPrime: return ref NPC.downedMechBoss3;
			case EventFlags.DefeatedPlantBoss: return ref NPC.downedPlantBoss;
			case EventFlags.DefeatedEmpressOfLight: return ref NPC.downedEmpressOfLight;
			case EventFlags.DefeatedFishron: return ref NPC.downedFishron;
			case EventFlags.DefeatedGolemBoss: return ref NPC.downedGolemBoss;
			case EventFlags.DefeatedHalloweenTree: return ref NPC.downedHalloweenTree;
			case EventFlags.DefeatedHalloweenKing: return ref NPC.downedHalloweenKing;
			case EventFlags.DefeatedChristmasTree: return ref NPC.downedChristmasTree;
			case EventFlags.DefeatedChristmasSantank: return ref NPC.downedChristmasSantank;
			case EventFlags.DefeatedChristmasIceQueen: return ref NPC.downedChristmasIceQueen;
			case EventFlags.DefeatedAncientCultist: return ref NPC.downedAncientCultist;
			case EventFlags.DefeatedMoonlord: return ref NPC.downedMoonlord;
			case EventFlags.DefeatedTowerSolar: return ref NPC.downedTowerSolar;
			case EventFlags.DefeatedTowerVortex: return ref NPC.downedTowerVortex;
			case EventFlags.DefeatedTowerNebula: return ref NPC.downedTowerNebula;
			case EventFlags.DefeatedTowerStardust: return ref NPC.downedTowerStardust;
			case EventFlags.DefeatedOldOnesArmyT1: return ref DD2Event.DownedInvasionT1;
			case EventFlags.DefeatedOldOnesArmyT2: return ref DD2Event.DownedInvasionT2;
			case EventFlags.DefeatedOldOnesArmyT3: return ref DD2Event.DownedInvasionT3;
		}

		throw new InvalidOperationException();
	}

	private static void OnSetEventFlagCleared(On_NPC.orig_SetEventFlagCleared orig, ref bool eventFlag, int gameEventId)
	{
		orig(ref eventFlag, gameEventId);

		if (skipTrackingEventCompletions)
		{
			return;
		}

		foreach (EventFlags flag in EventFlagsValues)
		{
			if (Unsafe.AreSame(ref eventFlag, ref GetExternalFlagValueRefOrNull(flag)))
			{
				CompleteEvent(flag, gameEventId);
				break;
			}
		}
	}
}
