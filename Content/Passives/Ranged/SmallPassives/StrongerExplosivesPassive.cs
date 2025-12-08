using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives;

internal class StrongerExplosivesPassive : Passive
{
	internal class StrongerExplosivesProjectile : GlobalProjectile 
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StrongerExplosivesPassive>(out float value))
			{
				projectile.damage = (int)(projectile.damage * (1 + value / 100f));
			}
		}
	}
}