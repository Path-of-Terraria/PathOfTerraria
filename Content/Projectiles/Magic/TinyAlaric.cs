using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Items.Gear.Weapons.Wand;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Magic;

public class TinyAlaric : ModProjectile
{
	public Player Owner => Main.player[Projectile.owner];

	ref float ArmRotation => ref Projectile.ai[0];
	ref float Timer => ref Projectile.ai[1];
	ref float VisualTimer => ref Projectile.ai[2];

	private float speedUpTimer = 0;

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(20, 38);
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		VisualTimer++;
		speedUpTimer--;

		if (Owner.dead || !Owner.active || Owner.HeldItem.type != ModContent.ItemType<TinyHat>())
		{
			Projectile.Kill();
			return;
		}

		Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center - new Vector2(24 * Owner.direction, 40 + MathF.Sin(VisualTimer * 0.08f) * 6), 0.3f);

		if (Main.rand.NextBool(3))
		{
			Vector2 dustVel = new Vector2(0, Main.rand.NextFloat(1.5f, 3)).RotatedByRandom(0.2f);
			Dust.NewDustPerfect(Projectile.BottomLeft + new Vector2(4 + Main.rand.Next(Projectile.width - 6), -2), DustID.BlueFlare, dustVel);
		}

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.spriteDirection = Projectile.direction = Main.MouseWorld.X < Owner.Center.X ? -1 : 1;

			ArmRotation = Projectile.AngleTo(Main.MouseWorld);
			Timer--;

			if (Timer <= 0 && Main.mouseLeft && !Owner.mouseInterface)
			{
				int type = Utils.SelectRandom<int>(Main.rand, ProjectileID.Fireball, ProjectileID.RayGunnerLaser, ProjectileID.MartianTurretBolt,
					ProjectileID.DiamondBolt, ProjectileID.PartyBullet);
				Vector2 vel = Projectile.DirectionTo(Main.MouseWorld) * 8;

				int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel, type, Projectile.damage, 1f, Main.myPlayer);
				Main.projectile[proj].friendly = true;
				Main.projectile[proj].hostile = false;
				Main.projectile[proj].extraUpdates = 2;
				Timer = speedUpTimer > 0 ? 12 : 25;
			}

			if (Main.mouseRight && Owner.GetModPlayer<AltUsePlayer>().AltFunctionAvailable && !Owner.mouseInterface)
			{
				Owner.GetModPlayer<AltUsePlayer>().SetAltCooldown(600, 300);
				speedUpTimer = 300;
			}
		}
	}

	public override void OnKill(int timeLeft)
	{
		SoundEngine.PlaySound(SoundID.Item25, Projectile.position);

		for (int i = 0; i < 20; ++i)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.width, Main.rand.NextBool() ? DustID.BlueFlare : DustID.BlueMoss);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		SpriteEffects effect = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		Vector2 baseDrawPos = Projectile.Center - Main.screenPosition;

		var bodyFrame = new Rectangle(14, 0, 20, 38);
		Main.EntitySpriteDraw(tex, baseDrawPos, bodyFrame, lightColor, 0f, bodyFrame.Size() / 2f, 1f, effect, 0);

		if (effect != SpriteEffects.None)
		{
			DrawRightArm(tex, baseDrawPos, lightColor);
			DrawLeftArm(tex, baseDrawPos, lightColor);
		}
		else
		{
			DrawLeftArm(tex, baseDrawPos, lightColor);
			DrawRightArm(tex, baseDrawPos, lightColor);
		}

		return false;
	}

	private void DrawLeftArm(Texture2D tex, Vector2 baseDrawPos, Color lightColor)
	{
		var armFrame = new Rectangle(0, 20, 12, 8);
		float armRot2 = ArmRotation - MathHelper.Pi;
		Main.EntitySpriteDraw(tex, baseDrawPos + new Vector2(-4, 5), armFrame, lightColor, armRot2, new Vector2(10, 4), 1f, SpriteEffects.None, 0);
	}

	private void DrawRightArm(Texture2D tex, Vector2 baseDrawPos, Color lightColor)
	{
		var armFrame = new Rectangle(0, 10, 12, 8);
		Main.EntitySpriteDraw(tex, baseDrawPos + new Vector2(4, 5), armFrame, lightColor, ArmRotation, new Vector2(0, 4), 1f, SpriteEffects.None, 0);
	}
}