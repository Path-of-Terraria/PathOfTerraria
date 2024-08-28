using System.Collections.Generic;
using PathOfTerraria.Common.NPCs.Components;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Effects;

/// <summary>
///     Provides registration and handles the spawning of NPC effects upon hit.
/// </summary>
/// <remarks>
///     This is useful for automatically spawning effects such as dusts, gores, etc.
/// </remarks>
public sealed class NPCHitEffects : NPCComponent
{
	public readonly struct GoreSpawnParameters
	{
		/// <summary>
		///     The type of gore to spawn.
		/// </summary>
		public readonly int Type;

		/// <summary>
		///     The minimum amount of gore to spawn.
		/// </summary>
		public readonly int MinAmount;

		/// <summary>
		///     The maximum amount of gore to spawn.
		/// </summary>
		public readonly int MaxAmount;

		/// <summary>
		///     An optional predicate to determine whether the dust should spawn or not.
		/// </summary>
		public readonly Func<NPC, bool>? Predicate;

		public GoreSpawnParameters(int type, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null)
		{
			Type = type;
			MinAmount = minAmount;
			MaxAmount = maxAmount;
			Predicate = predicate;
		}

		public GoreSpawnParameters(int type, int amount, Func<NPC, bool>? predicate = null) : this(type, amount, amount, predicate) { }
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
		) { }
	}

	/// <summary>
	///     Whether to spawn the party hat gore for town NPCs during a party or not.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>true</c>.
	/// </remarks>
	public bool SpawnPartyHatGore { get; set; } = true;

	/// <summary>
	///     The list of registered <see cref="DustSpawnParameters" /> for this component.
	/// </summary>
	public readonly List<DustSpawnParameters> DustPool = [];

	/// <summary>
	///     The list of registered <see cref="GoreSpawnParameters" /> for this component.
	/// </summary>
	public readonly List<GoreSpawnParameters> GorePool = [];

	/// <summary>
	///     Adds a specified amount of gore to the spawn pool from its name.
	/// </summary>
	/// <param name="name">The name of the gore to spawn.</param>
	/// <param name="amount">The amount of gore to spawn.</param>
	public void AddGore(string name, int amount = 1, Func<NPC, bool>? predicate = null)
	{
		int type = ModContent.Find<ModGore>(name).Type;

		AddGore(type, amount, predicate);
	}

	/// <summary>
	///     Adds a specified amount of gore to the spawn pool from its name.
	/// </summary>
	/// <param name="name">The name of the gore to spawn.</param>
	/// <param name="minAmount">The minimum amount of gore to spawn.</param>
	/// <param name="maxAmount">The maximum amount of gore to spawn.</param>
	public void AddGore(string name, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null)
	{
		int type = ModContent.Find<ModGore>(name).Type;

		AddGore(type, minAmount, maxAmount, predicate);
	}

	/// <summary>
	///     Adds a specified amount of gore to the spawn pool from its type.
	/// </summary>
	/// <param name="type">The type of the gore to spawn.</param>
	/// <param name="amount">The amount of gore to spawn.</param>
	public void AddGore(int type, int amount = 1, Func<NPC, bool>? predicate = null)
	{
		AddGore(type, amount, amount, predicate);
	}

	/// <summary>
	///     Adds a specified amount of gore to the spawn pool from its type.
	/// </summary>
	/// <param name="type">The type of the gore to spawn.</param>
	/// <param name="minAmount">The minimum amount of gore to spawn.</param>
	/// <param name="maxAmount">The maximum amount of gore to spawn.</param>
	public void AddGore(int type, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null)
	{
		GorePool.Add(new GoreSpawnParameters(type, minAmount, maxAmount, predicate));
	}

	/// <summary>
	///     Adds a specified amount of dust to the spawn pool from its name.
	/// </summary>
	/// <param name="name">The name of the dust to spawn.</param>
	/// <param name="amount">The amount of dust to spawn.</param>
	/// <param name="initializer">An optional initializer to set dust properties on spawn.</param>
	public void AddDust(string name, int amount = 1, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null)
	{
		int type = ModContent.Find<ModDust>(name).Type;

		AddDust(type, amount, amount, predicate, initializer);
	}

	/// <summary>
	///     Adds a specified amount of dust to the spawn pool from its type.
	/// </summary>
	/// <param name="name">The name of the dust to spawn.</param>
	/// <param name="amount">The amount of dust to spawn.</param>
	/// <param name="initializer">An optional initializer to set dust properties on spawn.</param>
	public void AddDust(int type, int amount = 1, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null)
	{
		AddDust(type, amount, amount, predicate, initializer);
	}

	/// <summary>
	///     Adds a specified amount of dust to the spawn pool from its name.
	/// </summary>
	/// <param name="name">The name of the dust to spawn.</param>
	/// <param name="minAmount">The minimum amount of dust to spawn.</param>
	/// <param name="maxAmount">The maximum amount of dust to spawn.</param>
	/// <param name="initializer">An optional initializer to set dust properties on spawn.</param>
	public void AddDust(string name, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null)
	{
		int type = ModContent.Find<ModDust>(name).Type;

		AddDust(type, minAmount, maxAmount, predicate, initializer);
	}

	/// <summary>
	///     Adds a specified amount of dust to the spawn pool from its type.
	/// </summary>
	/// <param name="type">The type of the dust to spawn.</param>
	/// <param name="minAmount">The minimum amount of dust to spawn.</param>
	/// <param name="maxAmount">The maximum amount of dust to spawn.</param>
	/// <param name="initializer">An optional initializer to set dust properties on spawn.</param>
	public void AddDust(int type, int minAmount, int maxAmount, Func<NPC, bool>? predicate = null, Action<Dust>? initializer = null)
	{
		DustPool.Add(new DustSpawnParameters(type, minAmount, maxAmount, predicate, initializer));
	}

	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (!Enabled || npc.life > 0 || Main.netMode == NetmodeID.Server)
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
			bool canSpawn = pool.Predicate == null ? true : pool.Predicate.Invoke(npc);

			if (pool.MinAmount <= 0 || !canSpawn)
			{
				continue;
			}

			int amount = Main.rand.Next(pool.MinAmount, pool.MaxAmount);

			for (int i = 0; i < amount; i++)
			{
				if (pool.Type <= 0)
				{
					continue;
				}

				Gore.NewGore(npc.GetSource_Death(), npc.position, npc.velocity, pool.Type);
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
			bool canSpawn = pool.Predicate == null ? true : pool.Predicate.Invoke(npc);

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