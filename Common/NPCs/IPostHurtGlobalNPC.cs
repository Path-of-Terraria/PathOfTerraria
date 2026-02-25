using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.NPCs;

#nullable enable

/// <summary>
/// Adds a hook that runs after an NPC is hit by an item or projectile. Only runs for player attacks.
/// </summary>
internal interface IPostHurtGlobalNPC
{
	internal class HitNPCHooks : ILoadable
	{
		public delegate void hook_OnPlayerHitNPCWithItem(Player player, Item sItem, NPC nPC, in NPC.HitInfo hit, int damageDone);
		public delegate void hook_OnHitByProjectile(Projectile projectile, NPC nPC, in NPC.HitInfo hit, int damageDone);

		public void Load(Mod mod)
		{
			MonoModHooks.Add(typeof(CombinedHooks).GetMethod(nameof(CombinedHooks.OnPlayerHitNPCWithItem)), DetourOnHitNPC);
			MonoModHooks.Add(typeof(CombinedHooks).GetMethod(nameof(CombinedHooks.OnHitNPCWithProj)), DetourOnHitProjectile);
		}

		public static void DetourOnHitProjectile(hook_OnHitByProjectile orig, Projectile projectile, NPC nPC, in NPC.HitInfo hit, int damageDone)
		{
			orig(projectile, nPC, hit, damageDone);

			Invoke(new DamageSources() { Projectile = projectile }, nPC, in hit, damageDone);
		}

		public static void DetourOnHitNPC(hook_OnPlayerHitNPCWithItem orig, Player player, Item sItem, NPC nPC, in NPC.HitInfo hit, int damageDone)
		{
			orig(player, sItem, nPC, hit, damageDone);

			Invoke(new DamageSources()
			{
				Player = player,
				Item = sItem,
			}, nPC, in hit, damageDone);
		}

		public void Unload()
		{
		}
	}

	/// <summary>
	/// Describes either a <see cref="Player"/> and <see cref="Item"/> hit or a <see cref="Projectile"/> hit source.
	/// </summary>
	public readonly struct DamageSources()
	{
		public readonly bool IsProjectileSource => Projectile is not null;

		public Player? Player { get; init; }
		public Item? Item { get; init; }
		public Projectile? Projectile { get; init; }
	}

	private static readonly GlobalHookList<GlobalNPC> Hook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(i => ((IPostHurtGlobalNPC)i).PostHurt));

	/// <summary>
	/// Runs after tModLoader's <see cref="CombinedHooks.OnPlayerHitNPCWithItem"/> or <see cref="CombinedHooks.OnHitNPCWithProj"/>. 
	/// <br/>Runs on client only.
	/// </summary>
	void PostHurt(DamageSources sources, NPC npc, in NPC.HitInfo hit, int damageDone);

	public static void Invoke(DamageSources sources, NPC npc, in NPC.HitInfo hit, int damageDone)
	{
		foreach (IPostHurtGlobalNPC g in Hook.Enumerate(npc))
		{
			g.PostHurt(sources, npc, hit, damageDone);
		}
	}
}
