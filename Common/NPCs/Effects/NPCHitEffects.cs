using System.Collections.Generic;
using PathOfTerraria.Common.NPCs.Components;
using Terraria.ID;

/*
 *	Maybe this component should be repurposed into a general hit effects component?
 *
 *	This would imply the capability of adding all sorts of effects upon NPC hit with
 *	custom conditions, such as death effects, hit effects, random hit effects, etc.
 *
 *	For reference, town NPCs spawn blood upon every hit, but only spawn gore upon death.
 *
 *	Ideally, you would register the effects like the following:
 *
 *	NPC.TryEnableComponent<NPCHitEffects>(c => {
 *		c.AddGore(..., 1, npc => npc.life <= 0);
 *		c.AddGore(..., 2, npc => npc.life <= 0);
 *		c.AddGore(..., 2, npc => npc.life <= 0);
 *
 *		c.AddDust(DustID.Blood, 20); // No predicate defaults to 'true'.
 *	});
 */
namespace PathOfTerraria.Common.NPCs.Effects;

/// <summary>
///     Provides registration and handles the spawning of gore effects upon NPC death.
/// </summary>
public sealed class NPCDeathEffects : NPCComponent
{
	public struct GoreSpawnParameters
	{
		/// <summary>
		///     The type of gore to spawn.
		/// </summary>
		public int Type;

		/// <summary>
		///     The amount of gore to spawn.
		/// </summary>
		public int Amount;

		public GoreSpawnParameters(int type, int amount)
		{
			Type = type;
			Amount = amount;
		}
	}

	public struct DustSpawnParameters
	{
		/// <summary>
		///     The type of gore to spawn.
		/// </summary>
		public int Type;

		/// <summary>
		///     The amount of gore to spawn.
		/// </summary>
		public int Amount;

		public Action<Dust>? Initializer;

		public DustSpawnParameters(int type, int amount, Action<Dust>? initializer = null)
		{
			Type = type;
			Amount = amount;
			Initializer = initializer;
		}
	}

	/// <summary>
	///     Whether to spawn the party hat gore for town NPCs during a party or not.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>true</c>.
	/// </remarks>
	public bool SpawnPartyHatGore { get; set; } = true;

	public readonly List<DustSpawnParameters> DustPool = [];

	public readonly List<GoreSpawnParameters> GorePool = [];

	/// <summary>
	///     Adds a specified amount of gore to the spawn pool from its name.
	/// </summary>
	/// <param name="name">The name of the gore to add.</param>
	/// <param name="amount">The amount of gore to add.</param>
	public void AddGore(string name, int amount = 1)
	{
		int type = Mod.Find<ModGore>(name).Type;

		AddGore(type, amount);
	}

	/// <summary>
	///     Adds a specified amount of gore to the spawn pool from its type.
	/// </summary>
	/// <param name="type">The type of the gore to add.</param>
	/// <param name="amount">The amount of gore to add.</param>
	public void AddGore(int type, int amount = 1)
	{
		GorePool.Add(new GoreSpawnParameters(type, amount));
	}

	/// <summary>
	///     Adds a specified amount of dust to the spawn pool from its type.
	/// </summary>
	/// <param name="type">The type of dust to add.</param>
	/// <param name="amount">The amount of dust to add.</param>
	/// <param name="initializer"></param>
	public void AddDust(int type, int amount = 1, Action<Dust>? initializer = null)
	{
		DustPool.Add(new DustSpawnParameters(type, amount, initializer));
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
			if (pool.Amount <= 0)
			{
				continue;
			}

			for (int i = 0; i < pool.Amount; i++)
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
			if (pool.Amount <= 0)
			{
				continue;
			}

			for (int i = 0; i < pool.Amount; i++)
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