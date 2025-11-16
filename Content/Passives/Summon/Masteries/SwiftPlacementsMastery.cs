using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class SwiftPlacementsMastery : Passive
{
	internal class SwiftPlacementsProjectile : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (projectile.sentry && source is EntitySource_Parent parentSource && parentSource.Entity is Player player)
			{
				if (player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SwiftPlacementsMastery>(out float value))
				{
					player.AddBuff(ModContent.BuffType<SwiftPlacementsBuff>(), 10 * 60);
				}
			}
		}
	}
}

