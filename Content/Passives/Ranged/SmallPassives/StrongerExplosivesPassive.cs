using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class StrongerExplosivesPassive : Passive
{
	internal class StrongerExplosivesProjectile : GlobalProjectile 
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is not EntitySource_ItemUse use) { return; }

			bool IsExplosive()
			{
				if (ProjectileID.Sets.Explosive[projectile.type]) { return true; }
				
				if (source is EntitySource_ItemUse_WithAmmo ammoUse)
				{
					Item ammo = ContentSamples.ItemsByType[ammoUse.AmmoItemIdUsed];
					if (ammo.ammo == AmmoID.Rocket) { return true; }
				}
				
				return false;
			}
			
			if (!IsExplosive()) { return; }
			if (!use.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StrongerExplosivesPassive>(out float value)) { return; }
			
			projectile.damage = (int)(projectile.damage * (1 + value / 100f));
		}
	}
}