using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class YryothIcicle : ModProjectile
{
	private static Asset<Texture2D>? glowmask;

	public override string Texture => $"{PoTMod.ModName}/Content/Conflux/CryoStalkerIcicle";

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 2;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new(18);
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = 1;
		Projectile.timeLeft = 240;
		Projectile.extraUpdates = 1;
		Projectile.aiStyle = -1;
	}

	public override void OnSpawn(IEntitySource source)
	{
		Projectile.frame = Main.rand.Next(2);
	}

	public override void AI()
	{
		if (Projectile.velocity == Vector2.Zero)
		{
			Projectile.Kill();
			return;
		}

		Projectile.rotation = Projectile.velocity.ToRotation()
			+ MathHelper.ToRadians(180f + (Projectile.frame == 0 ? 59f : 39f));
		Lighting.AddLight(Projectile.Center, ColorUtils.FromHexRgb(0x7ce8ff).ToVector3() * 0.8f);

		if (!Main.dedServ && Main.rand.NextBool(3))
		{
			Dust.NewDustPerfect(Projectile.Center, DustID.Ice, -Projectile.velocity * 0.08f, Scale: 0.8f).noGravity = true;
		}
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		target.AddBuff(BuffID.Chilled, 90);
	}

	public override void OnKill(int timeLeft)
	{
		if (Main.dedServ) { return; }

		SoundEngine.PlaySound(new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FrostMagic")
		{
			Volume = 0.15f,
			MaxInstances = 5,
			PitchVariance = 0.2f,
		}, Projectile.Center);

		for (int i = 0; i < 8; i++)
		{
			Dust.NewDustPerfect(Projectile.Center, DustID.Ice, Main.rand.NextVector2Circular(7f, 7f));
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		SpriteFrame spriteFrame = new(1, (byte)Main.projFrames[Type]) { PaddingX = 0, PaddingY = 0 };
		Rectangle frame = spriteFrame.With(0, (byte)Projectile.frame).GetSourceRectangle(texture);
		Vector2 origin = frame.Size() * 0.5f;
		Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White,
			Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);

		if ((glowmask ??= ModContent.Request<Texture2D>($"{Texture}_Glowmask")) is { IsLoaded: true, Value: { } glowTexture })
		{
			Main.EntitySpriteDraw(glowTexture, Projectile.Center - Main.screenPosition, frame, Color.White,
				Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
		}

		return false;
	}
}

internal sealed class YryothBite : ModProjectile
{
	public override string Texture => $"{PoTMod.ModName}/Content/Conflux/CryoStalkerIcicle";

	private int BossIndex => (int)Projectile.ai[0];

	public override void SetDefaults()
	{
		Projectile.Size = new(190);
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 18;
		Projectile.aiStyle = -1;
		Projectile.hide = true;
	}

	public override void AI()
	{
		if (BossIndex < 0 || BossIndex >= Main.maxNPCs || Main.npc[BossIndex] is not { active: true, ModNPC: GlacialBoss boss })
		{
			Projectile.Kill();
			return;
		}

		Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
		Projectile.Center = boss.Center + direction * 115f;
		Projectile.velocity = direction;

		if (!Main.dedServ)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 position = Projectile.Center + Main.rand.NextVector2Circular(85f, 85f);
				Dust.NewDustPerfect(position, DustID.Ice, direction * 4f, Scale: 1.3f).noGravity = true;
			}
		}
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		target.AddBuff(BuffID.Chilled, 150);
	}

	public override bool PreDraw(ref Color lightColor) => false;
}
