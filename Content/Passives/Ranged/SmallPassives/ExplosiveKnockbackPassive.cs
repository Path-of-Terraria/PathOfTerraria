using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives;

internal class ExplosiveKnockbackPassive : Passive
{
	internal class ExplosivesKnockbackProjectile : GlobalProjectile 
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (ProjectileID.Sets.Explosive[projectile.type] && source is EntitySource_ItemUse itemUse && itemUse.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ExplosiveKnockbackPassive>(out float value))
			{
				projectile.knockBack *= 1 + (value / 100f);
			}
		}
	}
}
