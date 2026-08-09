using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives;

internal class MoveIncreaseOnSummonPassive : Passive
{
	internal class MoveIncreaseOnSummonProjectile : GlobalProjectile
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (projectile.owner is < 0 or >= Main.maxPlayers)
			{
				return;
			}

			Player plr = Main.player[projectile.owner];
			if (plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MoveIncreaseOnSummonPassive>(out float value))
			{
				plr.AddBuff(ModContent.BuffType<HastySummonBuff>(), (int)(value * 60), false);
			}
		}
	}
}