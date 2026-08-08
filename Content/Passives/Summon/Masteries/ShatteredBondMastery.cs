using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Summoner;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class ShatteredBondMastery : Passive
{
	internal class ShatteredBondProjectile : GlobalProjectile
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.ModProjectile is not GrimoireSummon && entity.minion || entity.sentry;
		}

		public override void OnKill(Projectile projectile, int timeLeft)
		{
			if (projectile.TryGetOwner(out Player plr) && plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ShatteredBondMastery>(out float value))
			{
				ExplosionHitbox.QuickSpawn(projectile.GetSource_Death(), projectile, projectile.damage * 2, plr.whoAmI, new Vector2(240, 240), ExplosionSpawnInfo.PlayerOwned(plr.whoAmI));
			}
		}
	}
}