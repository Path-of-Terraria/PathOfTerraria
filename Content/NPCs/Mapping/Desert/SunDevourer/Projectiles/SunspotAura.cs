using System.Collections.Generic;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;

public sealed class SunspotAura : ModProjectile
{
	public const int LifeTime = 60 * 35;
	public const float MaxOpacity = 0.3f;

	public Vector2 Target => new(Projectile.ai[0], Projectile.ai[1]);
	public bool Active => Projectile.DistanceSQ(Target) < 10 * 10;

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(434);
		Projectile.timeLeft = LifeTime;
		Projectile.Opacity = MaxOpacity;
		Projectile.hide = true;
		Projectile.scale = 0.2f;
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		overPlayers.Add(index);
	}

	public override bool CanHitPlayer(Player target)
	{
		return false;
	}

	public override void AI()
	{
		Projectile.rotation += 0.01f;
		Projectile.Center = Vector2.Lerp(Projectile.Center, Target, 0.05f);

		if (Active && Projectile.timeLeft > 60)
		{
			Projectile.scale = MathHelper.Lerp(Projectile.scale, 1, 0.05f);

			if (!NPC.AnyNPCs(ModContent.NPCType<SunDevourerNPC>()))
			{
				Projectile.timeLeft = 60;
			}
		}

		if (Projectile.timeLeft <= 60)
		{
			float factor = Projectile.timeLeft / 60f;
			Projectile.scale = MathF.Min(factor, Projectile.scale);
			Projectile.Opacity = factor * MaxOpacity;
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White * Projectile.Opacity;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Queue<SunspotBatching.Sunspot> sunspot = Projectile.whoAmI % 4 == 0 ? SunspotBatching.SparseSunspots : SunspotBatching.FullSunspots;
		sunspot.Enqueue(new SunspotBatching.Sunspot(Projectile.Center, Projectile.scale, Projectile.rotation, Projectile.whoAmI));
		return false;
	}

	/// <summary>
	/// The projectile can't have this life drain functionality with lifeRegen in AI, so this does that instead.
	/// </summary>
	public class SunspotPlayer : ModPlayer
	{
		public override void UpdateBadLifeRegen()
		{
			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (projectile.ModProjectile is SunspotAura aura && aura.Active && projectile.DistanceSQ(Player.Center) < MathF.Pow(217 * projectile.scale, 2))
				{
					Player.lifeRegen -= 60;
				}
			}
		}
	}
}