using System.Collections.Generic;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class PoacherSummon : GrimoireSummon
{
	public override int BaseDamage => 12;

	protected bool WallClimb
	{
		get => Projectile.ai[1] == 1;
		set => Projectile.ai[1] = value ? 1 : 0;
	}

	public override void StaticDefaults()
	{
		Main.projFrames[Type] = 9;
	}

	public override void SetDefaults()
	{
		Projectile.width = 62;
		Projectile.height = 40;
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

		WallClimb = Main.tile[Projectile.Center.ToTileCoordinates()].WallType != WallID.None;

		if (Main.myPlayer == Projectile.owner)
		{
			if (!WallClimb)
			{
				Projectile.spriteDirection = Projectile.direction = -Math.Sign(Projectile.velocity.X);
				Projectile.velocity.X += Projectile.DirectionTo(Main.MouseWorld).X * 0.4f;
				Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -8, 8);
				Projectile.velocity.Y += 0.3f;
				Projectile.rotation = 0;

				if (Collision.SolidCollision(Projectile.BottomLeft, Projectile.width, 6) && MathF.Abs(Main.MouseWorld.X - Projectile.Center.X) < 20
					&& Projectile.Center.Y > Main.MouseWorld.Y + 60)
				{
					Projectile.velocity.Y = -8;
				}

				Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
			}
			else
			{
				Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.4f;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

				if (Projectile.velocity.LengthSquared() > 6 * 6)
				{
					Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 6;
				}
			}
		}
	}

	protected override void AltEffect()
	{
	}

	protected override void AnimateSelf()
	{
		if (WallClimb)
		{
			Projectile.frame = (int)((AnimationTimer += 0.2f) % 4);
		}
		else
		{
			AnimationTimer += 0.2f * Math.Abs(Projectile.velocity.X / 16f) + 0.2f;
			Projectile.frame = (int)(AnimationTimer % 5) + 4;
		}

		DrawOriginOffsetY = !WallClimb ? -40 : 0;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		return false;
	}

	public override Dictionary<int, int> GetRequiredParts()
	{
		return new Dictionary<int, int>()
		{
			{ ModContent.ItemType<OwlFeather>(), 1}, { ModContent.ItemType<BatWings>(), 1}, { ModContent.ItemType<ScorpionTail>(), 2 }
		};
	}
}
