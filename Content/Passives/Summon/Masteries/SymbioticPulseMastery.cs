using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class SymbioticPulseMastery : Passive
{
	internal class SymbioticPulseProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private int _timer = 0;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion;
		}

		public override bool PreAI(Projectile projectile)
		{
			if (projectile.TryGetOwner(out Player plr) && plr != null && plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SymbioticPulseMastery>(out float value))
			{
				_timer++;

				if (_timer > 30 * 60)
				{
					float healPercent = value / 100f;

					int type = ModContent.ProjectileType<HealingOrb>();
					Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, projectile.velocity, type, 0, 0, plr.whoAmI, healPercent);

					_timer = 0;
				}
			}

			return true;
		}
	}
}