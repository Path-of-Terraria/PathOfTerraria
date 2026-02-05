using System.Collections.Generic;
using PathOfTerraria.Common.NPCs.Components;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Effects;

#nullable enable

/// <summary>
///     Provides registration and handles the spawning of NPC effects upon hit.
/// </summary>
[Autoload(Side = ModSide.Client)]
public sealed class NPCHitEffects : NPCComponent
{
	public struct GoreSpawnParameters
	{
		/// <summary> The type of gore to spawn. </summary>
		public readonly int Type;
		/// <summary> The random amount of gore to spawn. </summary>
		public readonly (int Min, int Max) Amount;
		/// <summary> An optional predicate to determine whether the dust should spawn or not. </summary>
		public readonly Func<NPC, bool>? Predicate;
		/// <summary> The position values to use for spawning gores. </summary>
		public (Vector2 HitboxFactor, Vector2 Flat, Vector2 Random) Position = (new(1f, 1f), new(0f, 0f), new(0f, 0f));
		/// <summary> The velocity values to use for spawning gores. </summary>
		public (Vector2 InheritFactor, Vector2 Flat, Vector2 Random) Velocity = (new(1f, 1f), new(0f, 0f), new(1f, 1f));

		public GoreSpawnParameters(int type, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null)
		{
			Type = type;
			Amount = (minAmount, maxAmount);
			Predicate = predicate;
		}

		public GoreSpawnParameters(int type, int amount, Func<NPC, bool>? predicate = null) : this(type, amount, amount, predicate) { }

		public GoreSpawnParameters(string type, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null) : this(
			ModContent.Find<ModGore>(type).Type,
			minAmount,
			maxAmount,
			predicate
		)
		{ }

		public GoreSpawnParameters(string type, int amount, Func<NPC, bool>? predicate = null) : this(
			ModContent.Find<ModGore>(type).Type,
			amount,
			predicate
		)
		{ }
	}

	public readonly struct DustSpawnParameters
	{
		/// <summary>
		///     The type of dust to spawn.
		/// </summary>
		public readonly int Type;

		/// <summary>
		///     The minimum amount of duspt to spawn.
		/// </summary>
		public readonly int MinAmount;

		/// <summary>
		///     The maximum amount of dust to spawn.
		/// </summary>
		public readonly int MaxAmount;

		/// <summary>
		///     An optional predicate to determine whether the dust should spawn or not.
		/// </summary>
		public readonly Func<NPC, bool>? Predicate;

		/// <summary>
		///     An optional delegate to set dust properties on spawn.
		/// </summary>
		public readonly Action<Dust>? Initializer;

		public DustSpawnParameters(int type, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null)
		{
			Type = type;
			MinAmount = minAmount;
			MaxAmount = maxAmount;
			Predicate = predicate;
			Initializer = initializer;
		}

		public DustSpawnParameters(int type, int amount, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null) : this(
			type,
			amount,
			amount,
			predicate,
			initializer
		)
		{ }

		public DustSpawnParameters(string type, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null) : this(
			ModContent.Find<ModDust>(type).Type,
			minAmount,
			maxAmount,
			predicate,
			initializer
		)
		{ }

		public DustSpawnParameters(string type, int amount, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null) : this(
			ModContent.Find<ModDust>(type).Type,
			amount,
			predicate,
			initializer
		)
		{ }
	}

	/// <summary>
	/// Simply checks if this NPC is dead.
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	public static bool OnDeath(NPC npc)
	{
		return npc.life <= 0;
	}

	/// <summary>
	///     Whether to spawn the party hat gore for town NPCs during a party or not. Defaults to <c>true</c>.
	/// </summary>
	public bool SpawnPartyHatGore { get; set; } = true;

	/// <summary>
	///     The list of registered <see cref="DustSpawnParameters" /> instances in this component.
	/// </summary>
	public readonly List<DustSpawnParameters> DustPool = [];

	/// <summary>
	///     The list of registered <see cref="GoreSpawnParameters" /> instances in this component.
	/// </summary>
	public readonly List<GoreSpawnParameters> GorePool = [];

	/// <summary>
	///     Adds a new gore spawn entry to the spawn pool.
	/// </summary>
	/// <param name="parameters">The parameters that define how the gore should be spawned.</param>
	public void AddGore(in GoreSpawnParameters parameters)
	{
		GorePool.Add(parameters);
	}

	/// <summary>
	///     Adds a new dust spawn entry to the spawn pool.
	/// </summary>
	/// <param name="parameters">The parameters that define how the dust should be spawned.</param>
	public void AddDust(in DustSpawnParameters parameters)
	{
		DustPool.Add(parameters);
	}

	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (!Enabled || Main.dedServ)
		{
			return;
		}

		SpawnGore(npc);
		SpawnDust(npc);
	}

	private void SpawnGore(NPC npc)
	{
		if (GorePool.Count <= 0)
		{
			return;
		}

		foreach (GoreSpawnParameters pool in GorePool)
		{
			bool canSpawn = pool.Predicate?.Invoke(npc) ?? true;

			if (pool.Amount.Min <= 0 || !canSpawn)
			{
				continue;
			}

			int amount = Main.rand.Next(pool.Amount.Min, pool.Amount.Max);
			Vector2 npcSize = npc.Size;

			for (int i = 0; i < amount; i++)
			{
				if (pool.Type <= 0)
				{
					continue;
				}

				Vector2 pos = npc.position;
				// Add hitbox factor.
				pos += pool.Position.HitboxFactor * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat()) * npcSize;
				// If hitbox factor is below 1, move towards center.
				pos += npcSize * 0.5f * (Vector2.One - pool.Position.HitboxFactor);
				// Add flat.
				pos += pool.Position.Flat;
				// Add circular random.
				pos += Main.rand.NextVector2Circular(pool.Position.Random.X, pool.Position.Random.Y) * 0.5f;

				// Inherit, add flat, add circular random.
				Vector2 vel = default;
				vel += npc.velocity * pool.Velocity.InheritFactor;
				vel += pool.Velocity.Flat;
				vel += Main.rand.NextVector2Circular(pool.Velocity.Random.X, pool.Velocity.Random.Y) * 0.5f;

				var gore = Gore.NewGoreDirect(npc.GetSource_Death(), pos, vel, pool.Type);

				gore.position -= gore.Frame.GetSourceRectangle(TextureAssets.Gore[gore.type].Value).Size() * 0.5f;
			}
		}

		if (!npc.townNPC)
		{
			return;
		}

		int hat = npc.GetPartyHatGore();

		if (hat <= 0 || !SpawnPartyHatGore)
		{
			return;
		}

		Gore.NewGore(npc.GetSource_Death(), npc.position, npc.velocity, hat);
	}

	private void SpawnDust(NPC npc)
	{
		if (DustPool.Count <= 0)
		{
			return;
		}

		foreach (DustSpawnParameters pool in DustPool)
		{
			bool canSpawn = pool.Predicate?.Invoke(npc) ?? true;

			if (pool.MinAmount <= 0 || !canSpawn)
			{
				continue;
			}

			int amount = Main.rand.Next(pool.MinAmount, pool.MaxAmount);

			for (int i = 0; i < amount; i++)
			{
				if (pool.Type < 0)
				{
					continue;
				}

				var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, pool.Type);

				pool.Initializer?.Invoke(dust);
			}
		}
	}
}