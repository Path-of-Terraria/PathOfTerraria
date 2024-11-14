using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Content.Dusts;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Magic;

public class HomingProjectile : ModProjectile
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Projectiles/HomingProjectile";

	public override void SetDefaults()
	{
		Projectile.width = Projectile.height = 8;
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.timeLeft = 600;
	}

	public override void AI()
	{
		NPC closestNpc = null;
		float closestDistanceSq = float.MaxValue;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.friendly || npc.CountsAsACritter)
			{
				continue;
			}

			float distanceSQ = Projectile.Center.DistanceSQ(npc.Center);
			if (distanceSQ < closestDistanceSq)
			{
				closestNpc = npc;
				closestDistanceSq = distanceSQ;
			}
		}

		if (closestNpc != null)
		{
			var targetVel = Vector2.Normalize(closestNpc.Center - Projectile.Center);
			targetVel *= Projectile.velocity.Length();

			if (Vector2.Dot(Vector2.Normalize(Projectile.velocity), targetVel) > 0)
			{
				Projectile.velocity = Projectile.velocity.RotateTowards(targetVel, 0.015f); // how much it turns.
			}
		}

		if (Main.rand.NextBool(3))
		{
			Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height,
				ModContent.DustType<SparkleDust>(), Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f);
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int k = 0; k < 5; k++)
		{
			Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height,
				ModContent.DustType<SparkleDust>(), Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
		}

		SoundEngine.PlaySound(SoundID.Item25, Projectile.position);
	}
}