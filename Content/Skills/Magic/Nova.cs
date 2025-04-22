using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.SkillSpecials;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Skills.Magic;

public class Nova : Skill
{
	public enum NovaType : byte
	{
		Normal,
		Fire,
		Ice,
		Lightning,
	}

	public override int MaxLevel => 3;

	public static NovaType GetNovaType(Nova nova)
	{
		Player player = Main.LocalPlayer;
		SkillSpecial special = nova.Tree.Specialization;

		if (special is FireNova)
		{
			return NovaType.Fire;
		}
		else if (special is IceNova)
		{
			return NovaType.Ice;
		}
		else if (special is LightningNova)
		{
			return NovaType.Lightning;
		}

		return NovaType.Normal;
	}

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (15 - Level) * 60;
		ManaCost = 20 + 5 * Level;
		Duration = 0;
		WeaponType = ItemType.Magic;
	}

	public override void UseSkill(Player player, SkillBuff buff)
	{
		player.CheckMana((int)buff.ManaCost.ApplyTo(ManaCost), true);
		Timer = Cooldown;

		int damage = (int)buff.Damage.ApplyTo(player.HeldItem.damage * (2 + 0.5f * Level));
		var source = new EntitySource_UseSkill(player, this);
		NovaType type = GetNovaType(this);
		float knockback = 2f;

		if (type == NovaType.Fire)
		{
			knockback = 4f;
		}
		else if (type == NovaType.Lightning)
		{
			WeightedRandom<float> mult = new(Main.rand);
			mult.Add(1f, 1f);
			mult.Add(0.75f, 1f);
			mult.Add(0.5f, 1f);
			mult.Add(1.5f, 1f);
			mult.Add(2f, 1f);

			damage = (int)(damage * mult);
		}
		else if (type == NovaType.Ice)
		{
			damage = (int)(damage * 0.9f);
		}

		Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<NovaProjectile>(), damage, knockback, player.whoAmI, (int)type);
	}

	public override bool CanUseSkill(Player player)
	{
		return base.CanUseSkill(player) && player.HeldItem.CountsAsClass(DamageClass.Magic);
	}

	private class NovaProjectile : ModProjectile
	{
		private const int TotalRadius = 300;

		public override string Texture => "Terraria/Images/NPC_0";

		private int Spread => (int)((1 - Projectile.timeLeft / 30f) * TotalRadius);
		private NovaType NovaType => (NovaType)Projectile.ai[0];

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.timeLeft = 30;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			if (NovaType != NovaType.Lightning && NovaType != NovaType.Ice)
			{
				SpamNormalDust();
			}
			else if (NovaType == NovaType.Ice)
			{
				SpamIceDust();
			}
			else
			{
				SpamLightningDust();
			}
		}

		private void SpamNormalDust()
		{
			const float MaxDustIterations = 60;

			int dustType = NovaType switch
			{
				NovaType.Fire => DustID.Torch,
				_ => DustID.Astra
			};

			for (int i = 0; i < MaxDustIterations; ++i)
			{
				Vector2 spread = new Vector2(Spread, 0).RotatedBy(i / MaxDustIterations * MathHelper.TwoPi);
				float scale = 0.5f;

				if (NovaType == NovaType.Fire)
				{
					spread = spread.RotatedBy(Projectile.timeLeft / 8f * Spread / TotalRadius);
					spread *= Main.rand.NextFloat(0.9f, 1f);
					scale = Main.rand.NextFloat(0.75f, 1.7f);
				}

				Vector2 position = Projectile.Center + spread;
				Dust.NewDustPerfect(position, dustType, spread / Spread * 6, Scale: scale);
			}
		}

		private void SpamIceDust()
		{
			float dustIterations = Main.rand.Next(5, 8);

			if (Projectile.timeLeft % 2 != 0)
			{
				return;
			}

			for (int i = 0; i < dustIterations; ++i)
			{
				Vector2 spread = new Vector2(Spread, 0).RotatedBy(i / dustIterations * MathHelper.TwoPi);
				Vector2 lastSpread = new Vector2(Spread, 0).RotatedBy((i - 1) / dustIterations * MathHelper.TwoPi);
				float scale = Main.rand.NextFloat(0.9f, 1.5f) * (1 - Projectile.timeLeft / 30f);

				for (int j = 0; j < 10; ++j)
				{
					var useSpread = Vector2.Lerp(lastSpread, spread, j / 14f);
					int dustType = Main.rand.NextBool() ? DustID.SnowflakeIce : DustID.SnowSpray;
					var dust = Dust.NewDustPerfect(Projectile.Center + useSpread, dustType, useSpread.DirectionTo(Projectile.Center), Scale: scale);
					dust.noGravity = true;
					dust.fadeIn = 0.3f + j / 14f * 0.8f;
				}
			}
		}

		private void SpamLightningDust()
		{
			float dustIterations = Main.rand.Next(5, 8);

			if (Projectile.timeLeft % 3 != 0)
			{
				return;
			}

			for (int i = 0; i < dustIterations; ++i)
			{
				Vector2 spread = new Vector2(Spread, 0).RotatedBy(i / dustIterations * MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.2f);
				Vector2 lastSpread = new Vector2(Spread, 0).RotatedBy((i - 1) / dustIterations * MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.2f);
				float scale = Main.rand.NextFloat(0.9f, 1.5f) * (1 - Projectile.timeLeft / 30f);

				for (int j = 0; j < 13; ++j)
				{
					var useSpread = Vector2.Lerp(lastSpread, spread, j / 14f);
					var dust = Dust.NewDustPerfect(Projectile.Center + useSpread, DustID.Electric, useSpread.DirectionTo(Projectile.Center), Scale: scale);
					dust.noGravity = true;
					dust.fadeIn = 0.3f + j / 14f * 1.2f;
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float distanceSq = Projectile.Center.DistanceSQ(targetHitbox.Center());
			return distanceSq > MathF.Pow(Spread - 20, 2) && distanceSq < MathF.Pow(Spread + 20, 2);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (NovaType == NovaType.Fire)
			{
				target.AddBuff(BuffID.OnFire, 5 * 60);
			}
			else if (NovaType == NovaType.Lightning)
			{
				target.AddBuff(ModContent.BuffType<ShockDebuff>(), 5 * 60);
			}
			else if (NovaType == NovaType.Ice)
			{
				target.AddBuff(BuffID.Chilled, 5 * 60);
			}
		}
	}
}