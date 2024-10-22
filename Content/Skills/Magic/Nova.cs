using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.Magic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
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
	
	public override List<SkillPassive> Passives =>
	[
		new SkillPassiveAnchor(this),
		new LightningNovaSkillPassive(this),
		new FireNovaSkillPassive(this),
		new IceNovaSkillPassive(this)
	];

	public override string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetTexture();
	public override LocalizedText DisplayName => GetLocalization("Name", base.DisplayName);
	public override LocalizedText Description => GetLocalization("Description", base.Description);

	private LocalizedText GetLocalization(string postfix, LocalizedText def)
	{
		NovaType type = GetNovaType();
		return type switch
		{
			NovaType.Fire or NovaType.Ice or NovaType.Lightning => Language.GetText("Mods.PathOfTerraria.Skills." + Name + "." + type.ToString() + "." + postfix),
			_ => def,
		};
	}

	private string GetTexture()
	{
		return GetNovaType() switch
		{
			NovaType.Fire => "FireNova",
			NovaType.Lightning => "LightningNova",
			NovaType.Ice => "IceNova",
			_ => GetType().Name,
		};
	}

	private NovaType GetNovaType()
	{
		return GetNovaType(this);
	}

	public static NovaType GetNovaType(Nova nova)
	{
		Player player = Main.LocalPlayer;
		SkillPassivePlayer skillPassive = player.GetModPlayer<SkillPassivePlayer>();

		if (skillPassive.AllocatedPassives.TryGetValue(nova, out Dictionary<string, SkillPassive> passives))
		{
			if (passives.ContainsKey(nameof(FireNovaSkillPassive)))
			{
				return NovaType.Fire;
			}
			else if (passives.ContainsKey(nameof(IceNovaSkillPassive)))
			{
				return NovaType.Ice;
			}
			else if (passives.ContainsKey(nameof(LightningNovaSkillPassive)))
			{
				return NovaType.Lightning;
			}
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

	public override void UseSkill(Player player)
	{
		player.statMana -= ManaCost;
		Timer = Cooldown;

		int damage = (int)(player.HeldItem.damage * (2 + 0.5f * Level));
		var source = new EntitySource_Misc("NovaSkill");
		NovaType type = GetNovaType();
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