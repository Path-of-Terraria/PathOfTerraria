using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class ExplosiveSlugsMastery : Passive
{
	internal class ExplosiveSlugsProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private bool _bullet = false;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Item.useAmmo == AmmoID.Bullet)
			{
				_bullet = true;
			}
		}

		public override void OnHitNPC(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (_bullet && hit.Crit && proj.TryGetOwner(out Player plr) && plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ExplosiveSlugsMastery>(out float value))
			{
				ExplosionHitbox.QuickSpawn(proj.GetSource_OnHit(target), proj, (int)(damageDone * value / 100f), proj.owner, new Vector2(60, 60));
			}
		}
	}
}