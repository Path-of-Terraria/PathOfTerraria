using System.Collections.Generic;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class BatSummon : GrimoireSummon
{
	public override int BaseDamage => 5;

	public override void StaticDefaults()
	{
		Main.projFrames[Type] = 4;
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
		Projectile.timeLeft++;
		Projectile.rotation = Projectile.velocity.X * 0.1f;
		Projectile.spriteDirection = Projectile.direction = -Math.Sign(Projectile.velocity.X);

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.5f;
			
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
		Projectile.frame = (int)((AnimationTimer += 0.3f * (Projectile.velocity.Length() / 16f + 0.5f)) % 4);
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
		{
			Projectile.velocity.X = -oldVelocity.X * 0.9f;
		}

		if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
		{
			Projectile.velocity.Y = -oldVelocity.Y * 0.9f;
		}

		return false;
	}

	public override Dictionary<int, int> GetRequiredParts()
	{
		return new Dictionary<int, int>()
		{
			{ ModContent.ItemType<OwlFeather>(), 1}, { ModContent.ItemType<BatWings>(), 2}
		};
	}
}
