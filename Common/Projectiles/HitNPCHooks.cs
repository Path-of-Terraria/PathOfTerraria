using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Projectiles;

internal class HitNPCHooks : ILoadable
{
	/// <summary>
	/// Allows projectiles to run code after the projectile(s) are hit.
	/// </summary>
	internal interface IPostHitNPCProjectile
	{
		private static readonly GlobalHookList<GlobalProjectile> Hook = ProjectileLoader.AddModHook(GlobalHookList<GlobalProjectile>.Create(i => ((IPostHitNPCProjectile)i).PostHitNPC));

		void PostHitNPC(NPC target, in NPC.HitInfo hit, int damageDone);

		public static void Invoke(Projectile projectile, NPC target, in NPC.HitInfo hit, int damageDone)
		{
			foreach (IPostHitNPCProjectile g in Hook.Enumerate(projectile))
			{
				g.PostHitNPC(target, hit, damageDone);
			}
		}
	}

	public delegate void hook_OnHitNPC(Projectile projectile, NPC target, in NPC.HitInfo hit, int damageDone);

	public void Load(Mod mod)
	{
		MonoModHooks.Add(typeof(ProjectileLoader).GetMethod(nameof(ProjectileLoader.OnHitNPC)), DetourOnHitNPC);
	}

	public static void DetourOnHitNPC(hook_OnHitNPC orig, Projectile projectile, NPC target, in NPC.HitInfo hit, int damageDone)
	{
		orig(projectile, target, hit, damageDone);

		if (projectile.ModProjectile is IPostHitNPCProjectile post)
		{
			post.PostHitNPC(target, hit, damageDone);
		}

		IPostHitNPCProjectile.Invoke(projectile, target, in hit, damageDone);
	}

	public void Unload()
	{
	}
}
