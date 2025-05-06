using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

public sealed class BallLightning : ModProjectile
{
	public const int LifeTime = 60 * 8;

	private bool IsSmall => Projectile.ai[0] == 1;

	/// <summary>
	///		Gets or sets the index of the <see cref="Player"/> instance the projectile is homing towards. Shorthand for <c>Projectile.ai[1]</c>.
	/// </summary>
	public ref float Index => ref Projectile.ai[1];
	
	/// <summary>
	///		Gets the <see cref="Player"/> instance the projectile is homing towards. Shorthand for <c>Main.player[(int)Projectile.ai[1]]</c>.
	/// </summary>
	public Player Player => Main.player[(int)Index];

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.tileCollide = false;

		Projectile.width = 80;
		Projectile.height = 80;
		Projectile.timeLeft = LifeTime;
	}

	public override void OnKill(int timeLeft)
	{
		base.OnKill(timeLeft);

		for (int i = 0; i < 15; i++)
		{
			var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Scale: Main.rand.NextFloat(2, 4));
			dust.velocity.Y = Main.rand.NextFloat(-1, 1);
			dust.velocity *= 4f;
			dust.noGravity = true;
		}
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
		return true;
	}

	public override void AI()
	{
		if (IsSmall)
		{
			Projectile.Size = new Vector2(40, 40);
			Projectile.scale = 0.5f;
		}

		UpdateHoming();
		UpdateDustEffects();
	}

	private void UpdateHoming()
	{
		if (!Player.active || Player.dead || Player.ghost)
		{
			return;
		}

		const float MaxSpeed = 10;

		if (!IsSmall)
		{
			Projectile.velocity += Projectile.DirectionTo(Player.Center) * 0.4f;

			if (Projectile.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;
			}
		}
		else
		{
			Projectile.Center = Vector2.Lerp(Projectile.Center, Player.Center, 0.04f);
			Projectile.velocity = Vector2.Zero;
		}
	}

	private void UpdateDustEffects()
	{
		if (!Main.rand.NextBool(5))
		{
			return;
		}

		var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
		dust.velocity *= 2f;
		dust.noGravity = true;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		lightColor = new Color(235, 97, 52, 0);

		DrawProjectile(in lightColor);

		return false;
	}

	private void DrawProjectile(in Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);
		Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
		Vector2 origin = frame.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);
		SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		Main.EntitySpriteDraw(texture, position, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects);
	}
}