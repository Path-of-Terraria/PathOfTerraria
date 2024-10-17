using System.Collections.Generic;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class OwlSummon : GrimoireSummon
{
	public override int BaseDamage => 3;

	public override void StaticDefaults()
	{
		Main.projFrames[Type] = 13;
	}

	public override void SetDefaults()
	{
		Projectile.width = 26;
		Projectile.height = 34;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		if (!Channeling)
		{
			Despawning = true;
			return;
		}

		Projectile.timeLeft++;
		Projectile.rotation = Projectile.velocity.X * 0.03f;
		Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.2f;
			
			if (Projectile.velocity.LengthSquared() > 8 * 8)
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 8;
			}
		}
	}

	protected override void AltEffect()
	{
	}

	protected override void AnimateSelf()
	{
		Projectile.frame = (int)((AnimationTimer += 0.2f) % 5) + 8;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		return false;
	}

	public override Dictionary<int, int> GetRequiredParts()
	{
		return new Dictionary<int, int>()
		{
			{ ModContent.ItemType<OwlFeather>(), 2}, { ModContent.ItemType<BatWings>(), 1}
		};
	}
}
