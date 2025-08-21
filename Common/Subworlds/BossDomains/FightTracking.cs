#pragma warning disable CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.

using System.Collections.Generic;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public enum FightState
{
	Halted = -1,
	NotStarted,
	JustStarted,
	InProgress,
	EnemyVanished,
	JustCompleted,
	AlreadyCompleted,
}

/// <summary>
/// Keeps track of whether a fight's boss or other enemies have been spawned, defeated, or else.
/// </summary>
/// <param name="npcTypes">The type IDs making up the fight's enemy or enemies.</param>
public struct FightTracker(int[] npcTypes)
{
	public int[] NpcTypes { get; } = npcTypes;

	/// <summary> If true, the fight's start will not be signaled automatically when any tracked enemy is detected in the world. </summary>
	public bool ManualStart { get; init; }
	/// <summary> If true when enemies vanish without properly dying, the fight's data will be reset automatically, allowing a restart. </summary>
	public bool ResetOnVanish { get; init; }
	/// <summary> If true when enemies vanish without properly dying, the tracker's state will be put into <see cref="FightState.Halted"/> for the specified bit of time. </summary>
	public uint? HaltTimeOnVanish { get; init; }

	public bool Started { get; private set; }
	public bool AnnouncedStart { get; private set; }
	public bool Completed { get; private set; }
	public uint StartKillCount { get; private set; }
	public uint HaltTime { get; private set; }

	public void Reset()
	{
		this = new FightTracker(NpcTypes);
	}

	public void SignalStart()
	{
		Started = true;
		StartKillCount = GetCurrentKillCount();
	}

	public void SetHaltTime(uint ticks)
	{
		HaltTime = ticks;
	}

	// Could be made into an iterator method, but that may be more confusing than useful or convenient.
	public FightState UpdateState()
	{
		if (HaltTime > 0)
		{
			HaltTime--;
			return FightState.Halted;
		}

		if (!Started)
		{
			if (!ManualStart && AnyNPCs())
			{
				SignalStart();
			}
			else
			{
				return FightState.NotStarted;
			}
		}

		if (!AnnouncedStart)
		{
			AnnouncedStart = true;
			return FightState.JustStarted;
		}

		if (Completed)
		{
			return FightState.AlreadyCompleted;
		}

		if (!AnyNPCs())
		{
			if (GetCurrentKillCount() > StartKillCount)
			{
				Completed = true;
				return FightState.JustCompleted;
			}

			if (ResetOnVanish)
			{
				Reset();
			}

			if (HaltTimeOnVanish is uint ticks)
			{
				SetHaltTime(ticks);
			}

			return FightState.EnemyVanished;
		}

		return FightState.InProgress;
	}

	private readonly uint GetCurrentKillCount()
	{
		return LocalKillCountTracking.Get(NpcTypes);
	}

	private readonly bool AnyNPCs()
	{
		foreach (int type in npcTypes)
		{
			foreach (NPC activeNpc in Main.ActiveNPCs)
			{
				if (activeNpc.type == type) { return true; }
			}
		}

		return false;
	}
}

/// <summary>
/// Keeps track of all NPC kills performed in the world. Does not save anything.
/// </summary>
internal sealed class LocalKillCountTracking : ModSystem
{
	private static uint[] localKillCounts = [];

	public override void Load()
	{
		On_NPC.CountKillForBannersAndDropThem += static (orig, npc) =>
		{
			orig(npc);
			CountKill(npc);
		};
	}

	private static void CountKill(NPC npc)
	{
		if (npc?.type >= 0 && npc.type < NPCLoader.NPCCount)
		{
			Array.Resize(ref localKillCounts, NPCLoader.NPCCount);
			localKillCounts[npc.type]++;
		}
	}

	public static uint Get(int npcType)
	{
		return npcType >= 0 && npcType < localKillCounts.Length ? localKillCounts[npcType] : 0;
	}

	public static uint Get(ReadOnlySpan<int> npcTypes)
	{
		uint count = 0;

		foreach (int type in npcTypes)
		{
			count += Get(type);
		}

		return count;
	}
}