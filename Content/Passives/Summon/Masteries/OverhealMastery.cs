using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class OverhealMastery : Passive
{
	internal class OverhealPlayer : ModPlayer
	{
		public int overhealCooldown = 0;
		public const int OVERHEAL_COOLDOWN_DURATION = 12;

		public override void PostUpdate()
		{
			if (overhealCooldown > 0)
			{
				overhealCooldown--;
			}
		}
	}

	public override void OnLoad()
	{
		On_Player.Heal += HealTime;
	}

	private void HealTime(On_Player.orig_Heal orig, Player self, int amount)
	{
		int oldLife = self.statLife;

		orig(self, amount);

		if (oldLife + amount > self.statLifeMax2)
		{
			OverhealPlayer overhealPlayer = self.GetModPlayer<OverhealPlayer>();
			
			if (overhealPlayer.overhealCooldown > 0)
			{
				return;
			}

			int overheal = (oldLife + amount) - self.statLifeMax2;

			if (!self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<OverhealMastery>(out float value))
			{
				return;
			}

			// Set cooldown
			overhealPlayer.overhealCooldown = OverhealPlayer.OVERHEAL_COOLDOWN_DURATION;

			float damageBase = (overheal / (float)amount) * value / 100f;

			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.owner == self.whoAmI && proj.minion && !CustomProjectileSets.MultisegmentMinionProjectiles[proj.type])
				{
					int damage = (int)self.GetDamage(DamageClass.Summon).ApplyTo(damageBase * proj.damage);
					Projectile.NewProjectile(proj.GetSource_FromThis(), proj.Center, Vector2.Zero, ModContent.ProjectileType<OverhealPulseAura>(), damage, 0, self.whoAmI);
				}

				if (value <= 0)
				{
					break;
				}
			}
		}
	}
}