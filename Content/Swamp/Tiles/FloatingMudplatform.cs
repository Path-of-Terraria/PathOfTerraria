using PathOfTerraria.Common.Projectiles;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class FloatingMudplatform : ModProjectile, ISolidTopProjectile
{
	public const int MaxStandTime = 360;

	protected ref float StandingTimer => ref Projectile.ai[0];
	protected ref float WasStandingTimer => ref Projectile.ai[1];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 2;

		ISolidTopProjectile.SolidTopProjectileHooks.SolidTopOffsets[Type] = 8;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new(104, 36);
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.frame = Main.rand.Next(2);
		Projectile.direction = Projectile.spriteDirection = Main.rand.NextBool(2) ? -1 : 1;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Point16 topLeft = Projectile.TopLeft.ToTileCoordinates16();
		Point16 botRight = Projectile.BottomRight.ToTileCoordinates16();
		Projectile.timeLeft++;

		// Stop the projectile from updating if the section is unloaded
		// Otherwise, it'll either fall out of the world (desyncing) or break (desyncing)
		if (Main.sectionManager is null || !Main.sectionManager.TilesLoaded(topLeft.X, topLeft.Y, botRight.X, botRight.Y)) 
		{
			Projectile.position -= Projectile.velocity;
			return;
		}

		if (Collision.WetCollision(Projectile.Left - new Vector2(0, 4), Projectile.width, 8))
		{
			if (Collision.WetCollision(Projectile.Left - new Vector2(0, 12), Projectile.width, 8) && StandingTimer <= MaxStandTime)
			{
				Projectile.velocity.Y -= 0.05f;
			}
			else
			{
				Projectile.velocity *= 0.8f;
			}
		}
		else
		{
			Projectile.velocity.Y += 0.1f;
		}

		Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, -8, 8);

		if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
		{
			Projectile.Kill();
		}

		if (StandingTimer > MaxStandTime)
		{
			if (Projectile.velocity.Y < 3)
			{
				Projectile.velocity.Y += 0.07f;
			}
			else
			{
				Projectile.velocity.Y *= 0.98f;
			}
		}

		if (WasStandingTimer-- < 0)
		{
			StandingTimer = MathF.Max(StandingTimer - 3, 0);
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (!Main.dedServ)
		{
			for (int i = 0; i < 25; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Mud);
			}

			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
		}
	}

	bool ISolidTopProjectile.CanStandOn(Entity entity, Projectile projectile)
	{
		return CanStandOn(entity, projectile);
	}

	protected virtual bool CanStandOn(Entity entity, Projectile projectile)
	{
		float offset = ISolidTopProjectile.SolidTopProjectileHooks.SolidTopOffsets.TryGetValue(projectile.type, out float value) ? value : 2;
		return entity.velocity.Y >= 0 && entity.Bottom.Y < projectile.Hitbox.Y + 10 + offset;
	}

	void ISolidTopProjectile.UpdateSolidTop(Entity entity, Projectile projectile)
	{
		UpdateSolidTop(entity, projectile);
	}

	protected virtual void UpdateSolidTop(Entity entity, Projectile projectile)
	{
		Vector2 pos = projectile.position;

		projectile.velocity.Y += entity.velocity.Y * 0.1f;
		entity.velocity.Y = 0;
		float offset = ISolidTopProjectile.SolidTopProjectileHooks.SolidTopOffsets.TryGetValue(projectile.type, out float value) ? value : 2;
		var newPos = new Vector2(entity.position.X, projectile.Hitbox.Top + offset - entity.height);

		if (!Collision.SolidCollision(newPos, entity.width, entity.height))
		{
			entity.position = newPos;
		}

		if (projectile.velocity.Y > 0)
		{
			entity.position.Y += projectile.velocity.Y;
		}

		StandingTimer++;
		WasStandingTimer = 5;
	}
}

internal sealed class BrittleFloatingMudplatform : FloatingMudplatform
{
	private ref float HideTimer => ref Projectile.ai[2];

	public override void AI()
	{
		base.AI();

		if (StandingTimer > MaxStandTime - 80 && HideTimer <= 0)
		{
			StandingTimer++;
			WasStandingTimer = 5;
		}

		if (StandingTimer > MaxStandTime && HideTimer <= 0)
		{
			HideTimer = 600;

			for (int i = 0; i < 3; ++i)
			{
				OnKill(0); // Display vfx for break without dying
			}
		}

		if (HideTimer > 0)
		{
			HideTimer--;
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.5f, 0.05f);
		}
		else
		{
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1, 0.05f);
		}
	}

	protected override void UpdateSolidTop(Entity entity, Projectile projectile)
	{
		base.UpdateSolidTop(entity, projectile);

		StandingTimer += 3;
	}

	protected override bool CanStandOn(Entity entity, Projectile projectile)
	{
		return HideTimer <= 0 && base.CanStandOn(entity, projectile);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 pos = Projectile.Center - Main.screenPosition;

		if (StandingTimer > MaxStandTime - 80)
		{
			pos += Main.rand.NextVector2Circular(3, 3);
		}

		Color color = lightColor * Projectile.Opacity;
		Rectangle frame = tex.Frame(1, 2, 0, Projectile.frame, 0, 0);
		Main.spriteBatch.Draw(tex, pos, frame, color, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

		return false;
	}
}