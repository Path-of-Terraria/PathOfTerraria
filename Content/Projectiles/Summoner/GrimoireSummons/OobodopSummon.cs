using System.Collections.Generic;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class OobodopSummon : GrimoireSummon
{
	public override int BaseDamage => 55;

	private ref float Timer => ref Projectile.ai[1];
	private ref float LavaAccrued => ref Projectile.ai[2];

	public override void StaticDefaults()
	{
		Main.projFrames[Type] = 1;
	}

	public override void SetDefaults()
	{
		Projectile.width = 28;
		Projectile.height = 28;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.extraUpdates = 1;
	}

	public override bool? CanCutTiles()
	{
		return true;
	}

	public override void AI()
	{
		Lighting.AddLight(Projectile.Center, TorchID.Torch);

		Projectile.timeLeft++;
		Projectile.rotation = Projectile.velocity.ToRotation();
		Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f + LavaAccrued * 0.25f, 0.1f);

		Timer++;

		if (Main.rand.NextBool(3, 5))
		{
			int dustType = Utils.SelectRandom(Main.rand, 6, 259, 158);
			Vector2 dustOff = new Vector2(Main.rand.NextFloat(Projectile.height) - Projectile.height / 2, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
			Dust.NewDustPerfect(Projectile.Center + dustOff, dustType, -Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(0.3f, 0.6f));
		}

		if (Projectile.lavaWet)
		{
			LavaAccrued = MathHelper.Min(LavaAccrued + 0.05f, 4);
		}
		else if (Projectile.wet)
		{
			Projectile.Kill();
			return;
		}

		if (Main.myPlayer == Projectile.owner)
		{
			if (Timer % 25 == 1)
			{
				Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * (8 - LavaAccrued);
			}
		}
	}

	protected override void AltEffect()
	{
		if (LavaAccrued > 0)
		{
			int type = ModContent.ProjectileType<ExplosionHitboxFriendly>();
			Terraria.DataStructures.IEntitySource source = Projectile.GetSource_FromThis();
			int damage = (int)MathF.Max(Projectile.damage * LavaAccrued, 2);

			Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, type, damage, 6f, Projectile.owner, 100 * LavaAccrued, 100 * LavaAccrued);

			for (int i = 0; i < 8; ++i)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(2, 5);
				Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
			}

			for (int i = 0; i < 20; ++i)
			{
				Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1, 5));
			}

			for (int i = 0; i < 30; ++i)
			{
				int dustType = Utils.SelectRandom(Main.rand, 6, 259, 158);
				Dust.NewDustPerfect(Projectile.Center, dustType, -Projectile.velocity.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.9f, 1.2f));
			}

			SoundEngine.PlaySound(SoundID.Item14 with { PitchRange = (-0.8f, 0.2f), Volume = 0.7f }, Projectile.Center);

			LavaAccrued = 0;
			Timer = 0;
		}
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

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		LavaAccrued = MathHelper.Min(LavaAccrued + 0.2f, 4);

		target.AddBuff(BuffID.OnFire, 60);
	}

	public override Dictionary<int, int> GetRequiredParts()
	{
		return new Dictionary<int, int>()
		{
			{ ModContent.ItemType<SoulfulAsh>(), 1 }, { ModContent.ItemType<FlamingEye>(), 3 }
		};
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 eye = Projectile.DirectionTo(Main.MouseWorld) * 8;
		Vector2 drawPos = Projectile.Center - Main.screenPosition;
		Color color = lightColor * Projectile.Opacity;
		
		Main.EntitySpriteDraw(tex, drawPos, new Rectangle(0, 0, 28, 28), color, Projectile.rotation, new(14), Projectile.scale, SpriteEffects.None, 0);
		Main.EntitySpriteDraw(tex, drawPos + eye, new Rectangle(8, 30, 12, 6), color, Projectile.rotation, new(6, 3), Projectile.scale, SpriteEffects.None, 0);

		return false;
	}
}
