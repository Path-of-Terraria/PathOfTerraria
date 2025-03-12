using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.SunDevourerDomain;

public sealed class SunDevourerEruptionProjectile : ModProjectile
{
	private static readonly VertexStrip Strip = new();
	
	/// <summary>
	///		Gets or sets the timer of the projectile. Shorthand for <c>Projectile.ai[0]</c>.
	/// </summary>
	public ref float Timer => ref Projectile.ai[0];

	/// <summary>
	///		Gets or sets the index of the <see cref="Player"/> instance the projectile is homing towards. Shorthand for <c>Projectile.ai[1]</c>.
	/// </summary>
	public ref float Index => ref Projectile.ai[1];

	/// <summary>
	///		Gets the <see cref="Player"/> instance the projectile is homing towards. Shorthand for <c>Main.player[(int)Projectile.ai[1]]</c>.
	/// </summary>
	public Player Player => Main.player[(int)Index];
	
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.projFrames[Type] = 2;
		
		ProjectileID.Sets.TrailingMode[Type] = 3;
		ProjectileID.Sets.TrailCacheLength[Type] = 20;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.friendly = false;
		Projectile.hostile = true;

		Projectile.width = 16;
		Projectile.height = 16;
	}

	public override void OnSpawn(IEntitySource source)
	{
		base.OnSpawn(source);

		Projectile.frame = Main.rand.Next(2);
	}

	public override void OnKill(int timeLeft)
	{
		base.OnKill(timeLeft);

		for (var i = 0; i < 5; i++)
		{
			var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FlameBurst);

			dust.velocity *= 2f;
			
			dust.noGravity = true;
		}

		Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), GoreID.Smoke1);
		Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), GoreID.Smoke2);
		Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), GoreID.Smoke3);
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Vector2.UnitY, 4f, 6f, 30, 1000f, "SunDevourer"));
		
		Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
		
		return true;
	}

	public override void AI()
	{
		base.AI();

		Timer++;

		Projectile.velocity.X += MathF.Cos(Timer * 0.1f) * 0.01f;
		
		UpdateHoming();
		UpdateGravity();
		UpdateDustEffects();
	}

	private void UpdateHoming()
	{
		if (!Player.active || Player.dead || Player.ghost)
		{
			return;
		}
		
		var direction = MathF.Sign(Player.Center.X - Projectile.Center.X);

		Projectile.velocity.X += direction * 0.1f;
	}
	
	private void UpdateDustEffects()
	{
		if (!Main.rand.NextBool(5))
		{
			return;
		}
		
		var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FlameBurst);

		dust.velocity *= 2f;
			
		dust.noGravity = true;
	}

	private void UpdateGravity()
	{
		Projectile.velocity.Y += 0.3f;

		if (Projectile.velocity.Y < 12f)
		{
			return;
		}

		Projectile.velocity.Y = 12f;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		lightColor = new Color(235, 97, 52, 0);

		DrawProjectileTrail();
		DrawProjectile(in lightColor);
		
		return false;
	}

	private void DrawProjectile(in Color lightColor)
	{
		var texture = TextureAssets.Projectile[Type].Value;

		var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);

		var frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
		var origin = frame.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);

		var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		Main.EntitySpriteDraw(texture, position, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects);
	}

	private void DrawProjectileTrail()
	{
		var data = GameShaders.Misc["FlameLash"];
        
		data.UseSaturation(-2f);
		data.UseOpacity(10f);
        
		data.Apply();

		Strip.PrepareStripWithProceduralPadding
		(
			Projectile.oldPos,
			Projectile.oldRot, 
			static (progress) => new Color(235, 97, 52, 0) * progress,
			static (progress) => MathHelper.SmoothStep(32f, 0f, progress), 
			-Main.screenPosition + Projectile.Size / 2f
		);
		
		Strip.DrawTrail();
		
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	}
}