using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MonoMod.Cil;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Config;

#nullable enable
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE2003 // Blank line required between block and subsequent statement

namespace PathOfTerraria.Common.Encounters;

/// <summary> Description used to create an enemy encounter. </summary>
internal record struct EncounterDescription()
{
	/// <summary> An identifier for this encounter. Does not do anything on its own. </summary>
	public required string Identifier { get; set; } = string.Empty;
	/// <summary> The encounter's activation center in world-space (pixels). </summary>
	public required Vector2 ActivationOrigin { get; set; }
	/// <summary> The encounter's activation range in world-space (pixels). </summary>
	[Range(64f, 4096f), Increment(64f)]
	public required float ActivationRange { get; set; }
	/// <summary> The encounter's spawn area in tile-space. This will be supplied to all spawns with zeroed <see cref="SpawnPlacement.Area"/>. </summary>
	public required Rectangle SpawnArea { get; set; }
	/// <summary> The encounter's spawn center in tile-space. This will be supplied to all spawns with zeroed <see cref="SpawnPlacement.AreaOrigin"/>. </summary>
	public required Point16 SpawnOrigin { get; set; }
	/// <summary> The waves that this encounter consists of. </summary>
	public required EncounterWave[] Waves { get; set; }
	/// <summary> If set together with <see cref="SceneEffectPriority"/>, overrides played music when active and near. </summary>
	[JsonConverter(typeof(EntityDefinitionJsonConverter))]
	public MusicDefinition Music { get; set; } = new(0);
	/// <summary> The priority to use for effects such as <see cref="Music"/>. </summary>
	[JsonConverter(typeof(StringEnumConverter)), DefaultValue(SceneEffectPriority.None)]
	public SceneEffectPriority SceneEffectPriority { get; set; }

	/// <summary> Verifies that the description is sane and usable. Throws exceptions if it is not. </summary>
	public readonly void Verify()
	{
		if (Waves == null) { throw new ArgumentNullException(nameof(Waves)); }
		if (Waves.Length == 0) { throw new ArgumentOutOfRangeException(nameof(Waves)); }
		if (Identifier == null) { throw new ArgumentNullException(nameof(Identifier)); }
		if (SpawnArea.Width == 0 | SpawnArea.Height == 0) { throw new ArgumentOutOfRangeException(nameof(SpawnArea)); }
	}
}

internal record struct EncounterWave()
{
	/// <summary> The enemies to spawn. </summary>
	public required EnemySpawn[] Spawns { get; set; }
}

/// <summary> An encounter instance's activation state. </summary>
public enum EncounterState
{
	NotStarted,
	InProgress,
	Completed,
}

/// <summary> Information regarding an encounter instance. </summary>
internal record struct EncounterInstance()
{
	/// <summary> The activation state. </summary>
	public EncounterState State { get; set; } = EncounterState.NotStarted;
	/// <summary> Whether the encounter is paused. </summary>
	public bool IsPaused { get; set; }
	/// <summary> The currently active wave. </summary>
	public int WaveIndex { get; set; }
	/// <summary> The amount of enemies that have been spawned for the current wave. </summary>
	public int EnemiesSpawnedThisWave { get; set; }
}

/// <summary> A versioned handle for an encounter instance. </summary>
internal record struct Encounter(uint Index, uint Version) : IHandle
{
	/// <summary> Returns whether an encounter matching this handle still exists. Check for this often. </summary>
	public readonly bool IsValid => EnemyEncounters.encounters.IsValid(this);

	/// <summary> An immutable reference to this encounter's instance data. </summary>
	public readonly ref readonly EncounterInstance Instance => ref EnemyEncounters.encounters.Get(this).Instance;

	/// <summary> An immutable reference to this encounter's data. </summary>
	public readonly ref readonly EncounterDescription Description => ref EnemyEncounters.encounters.Get(this).Description;

	/// <summary> Starts (and unpauses) this encounter. </summary>
	public readonly void Start()
	{
		SetPaused(false);

		ref EnemyEncounters.InstanceData data = ref EnemyEncounters.encounters.Get(this);
		data.Instance.State = EncounterState.InProgress;
	}

	/// <summary> Marks this encounter as completed. </summary>
	public readonly void Complete()
	{
		ref EnemyEncounters.InstanceData data = ref EnemyEncounters.encounters.Get(this);
		data.Instance.State = EncounterState.Completed;
	}

	/// <summary> Resets this encounter. </summary>
	public readonly void Reset()
	{
		ref EnemyEncounters.InstanceData data = ref EnemyEncounters.encounters.Get(this);
		data = new() { Description = data.Description };
	}

	/// <summary> Removes this encounter if it is valid, returning whether that was the case. </summary>
	public readonly bool Remove()
	{
		return EnemyEncounters.encounters.Remove(this);
	}

	/// <summary> Pauses or unpauses this encounter. </summary>
	public readonly void SetPaused(bool value)
	{
		ref EnemyEncounters.InstanceData data = ref EnemyEncounters.encounters.Get(this);
		data.Instance.IsPaused = value;
	}

	/// <summary> Offsets this encounter's origins, area, and manual spawns, towards the given position. </summary>
	public readonly void MoveEverythingTo(Point16 targetTilePos)
	{
		EncounterDescription dsc = Description;
		Point16 tileOffset = targetTilePos - dsc.SpawnOrigin;
		Vector2 worldOffset = tileOffset.ToWorldCoordinates();

		dsc.SpawnOrigin = targetTilePos;
		dsc.SpawnArea = dsc.SpawnArea with { X = dsc.SpawnArea.X + tileOffset.X, Y = dsc.SpawnArea.Y + tileOffset.Y };
		dsc.ActivationOrigin += worldOffset;

		foreach (ref EncounterWave wave in dsc.Waves.AsSpan())
		{
			foreach (ref EnemySpawn spawn in wave.Spawns.AsSpan())
			{
				if (spawn.SpawnPosition is Vector2 spawnPos)
				{
					spawn.SpawnPosition = spawnPos + worldOffset;
				}
			}
		}

		ModifyDescription(dsc);
	}

	/// <summary> Performs a checked description mutation. </summary>
	public readonly void ModifyDescription(EncounterDescription description)
	{
		description.Verify();

		ref EnemyEncounters.InstanceData data = ref EnemyEncounters.encounters.Get(this);

		data.Description = description;

		if (data.Instance.WaveIndex >= data.Description.Waves.Length)
		{
			data.Instance.WaveIndex = data.Description.Waves.Length - 1;
			data.Instance.EnemiesSpawnedThisWave = 0;
		}
	}
}

internal sealed class EnemyEncounters : ModSystem
{
	internal struct InstanceData()
	{
		public required EncounterDescription Description;
		public EncounterInstance Instance = new();
		public uint SpawnCooldown;
	}

	// Wraps GenerationalArena's iterator to keep it an implementation detail. Enumerator methods do not cut it.
	public ref struct Iterator()
	{
		private GenerationalArena<Encounter, InstanceData>.Iterator inner = encounters.GetEnumerator();
		public readonly Encounter Current => inner.Current;

		public bool MoveNext() { return inner.MoveNext(); }
		public readonly Iterator GetEnumerator() { return this; }
	}

	private sealed class NpcLogic : GlobalNPC
	{
		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			// Reset mapping for this slot.
			enemyToEncounterMapping[npc.whoAmI] = default;
		}
	}

	internal static readonly GenerationalArena<Encounter, InstanceData> encounters = new(initialCapacity: 16);
	/// <summary> Maps spawned enemies to the encounter that spawned them. </summary>
	private static readonly Encounter[] enemyToEncounterMapping = new Encounter[Main.maxNPCs + 1];

	public static uint Count => encounters.Count;
	public static uint Capacity => encounters.Capacity;

	static EnemyEncounters()
	{
		
	}

	public override void Load()
	{
		base.Load();

		MonoModHooks.Modify(typeof(SceneEffectLoader).GetMethod(nameof(SceneEffectLoader.UpdateMusic)), SceneEffectLoaderUpdateMusicInjection);
	}

	public override void PostUpdateWorld()
	{
		UpdateEncounters();
	}

	public override void ClearWorld()
	{
		RemoveAllEncounters();
	}

	/// <summary> Creates an encounter with the given parameters. </summary>
	public static Encounter CreateEncounter(in EncounterDescription description)
	{
		Encounter encounter = encounters.Put(new InstanceData
		{
			Description = description,
		});
		return encounter;
	}

	/// <summary> Iterates all registered encounters, including those completed and not yet started. </summary>
	public static Iterator IterateEncounters() { return new(); }

	/// <summary> Removes all encounters that exist. </summary>
	public static void RemoveAllEncounters()
	{
		encounters.Clear();
	}

	private static void UpdateEncounters()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		StartEncounters();
		SpawnEnemies();
		ProgressEncounters();
		DebugEncounters();
	}

	private static void StartEncounters()
	{
		// Begin encounters approached by a player.
		foreach (Encounter encounter in encounters)
		{
			ref InstanceData data = ref encounters.Get(encounter);

			if (data.Instance.State != EncounterState.NotStarted || data.Instance.IsPaused) { continue; }

			ref readonly EncounterDescription description = ref data.Description;
			float sqrRange = description.ActivationRange * description.ActivationRange;

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Center.DistanceSQ(description.ActivationOrigin) <= sqrRange)
				{
					encounter.Start();
					break;
				}
			}
		}
	}

	private static void SpawnEnemies()
	{
		IEntitySource? source = null;

		foreach (Encounter encounter in encounters)
		{
			ref InstanceData data = ref encounters.Get(encounter);
			ref readonly EncounterDescription description = ref data.Description;
			ref readonly EncounterWave wave = ref description.Waves[data.Instance.WaveIndex];

			if (data.Instance.State != EncounterState.InProgress) { continue; }
			if (data.SpawnCooldown > 0 && --data.SpawnCooldown > 0) { continue; }
			if (data.Instance.IsPaused) { continue; }

			// Spawn enemies for the current wave.
			for (int enemyIndexInWave = data.Instance.EnemiesSpawnedThisWave; enemyIndexInWave < wave.Spawns.Length; enemyIndexInWave++)
			{
				EnemySpawn spawn = wave.Spawns[enemyIndexInWave];

				// Supply overrides for placement.
				if (spawn.SpawnPlacement is { } placement)
				{
					if (placement.Area == default) { placement.Area = description.SpawnArea; }
					if (placement.AreaOrigin == default) { placement.AreaOrigin = description.SpawnOrigin; }
					if (placement.CollisionSize == default) { placement.CollisionSize = ContentSamples.NpcsByNetId[spawn.NpcType.Type].Size.ToPoint(); }

					spawn.SpawnPlacement = placement;
				}

				source ??= Entity.GetSource_NaturalSpawn();

				if (!EnemySpawning.TrySpawningEnemy(source, in spawn, out NPC npc))
				{
					break;
				}

				data.Instance.EnemiesSpawnedThisWave++;
				data.SpawnCooldown += spawn.CooldownInTicks;
				enemyToEncounterMapping[npc.whoAmI] = encounter;

				if (data.SpawnCooldown > 0)
				{
					break;
				}
			}
		}
	}

	private static void ProgressEncounters()
	{
		foreach (Encounter encounter in encounters)
		{
			ref InstanceData data = ref encounters.Get(encounter);
			ref readonly EncounterDescription description = ref data.Description;
			ref readonly EncounterWave wave = ref description.Waves[data.Instance.WaveIndex];

			if (data.Instance.State != EncounterState.InProgress) { continue; }
			if (data.Instance.IsPaused) { continue; }

			// Switch to next wave or end the encounter if all of its spawned enemies have been killed.
			if (data.Instance.EnemiesSpawnedThisWave >= wave.Spawns.Length)
			{
				int livingCount = CountLivingEnemiesFromEncounter(encounter);

				if (livingCount == 0)
				{
					if (data.Instance.WaveIndex + 1 >= description.Waves.Length)
					{
						encounter.Complete();
					}
					else
					{
						NextWave(encounter);
					}
				}
			}
		}
	}

	private static void NextWave(Encounter encounter)
	{
		ref InstanceData data = ref encounters.Get(encounter);
		data.Instance.WaveIndex++;
		data.Instance.EnemiesSpawnedThisWave = 0;
	}

	private static int CountLivingEnemiesFromEncounter(Encounter encounter)
	{
		int result = 0;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (enemyToEncounterMapping[npc.whoAmI] == encounter)
			{
				result += 1;
			}
		}

		return result;
	}

	private static void SceneEffectLoaderUpdateMusicInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Go to the very bottom, just before 'ret'.
		il.Index = il.Instrs.Count - 1;

		ILUtils.HijackIncomingLabels(il);
		il.EmitLdarg(1);
		il.EmitLdarg(2);
		il.EmitDelegate(UpdateMusic);
	}

	private static void UpdateMusic(ref int music, ref SceneEffectPriority priority)
	{
		Player localPlayer = Main.LocalPlayer;

		foreach (Encounter encounter in encounters)
		{
			ref readonly InstanceData data = ref encounters.Get(encounter);

			if (data.Instance.State != EncounterState.InProgress) { continue; }
			if (data.Description.Music.Type <= 0) { continue; }
			if (data.Description.SceneEffectPriority < priority) { continue; }

			float increasedSqrRange = MathF.Pow(data.Description.ActivationRange * 3f, 2f);
			if (localPlayer.DistanceSQ(data.Description.ActivationOrigin) > increasedSqrRange) { continue; }

			priority = data.Description.SceneEffectPriority;
			music = data.Description.Music.Type;
		}
	}

	[Conditional("DEBUG")]
	private static void DebugEncounters()
	{
		
	}
}
