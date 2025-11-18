using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class OverhealMastery : Passive
{
	internal static void Overheal(Player self, int overheal, int totalHeal)
	{
		if (self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<OverhealMastery>(out float value))
		{
			float damageBase = (overheal / (float)totalHeal) * value / 100f;

			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.owner == self.whoAmI && proj.minion)
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