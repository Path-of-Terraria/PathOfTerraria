using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Passives.Summon.Masteries;
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
			if (source is EntitySource_Parent { Entity: Player plr })
			{
				TryFunctionality(plr);
			}
			else if (source is EntitySource_Parent { Entity: Projectile proj } && proj.TryGetOwner(out Player projOwner))
			{
				TryFunctionality(projOwner);
			}
		}

		private static void TryFunctionality(Player plr)
		{
			if (plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MoveIncreaseOnSummonPassive>(out float value))
			{
				plr.AddBuff(ModContent.BuffType<HastySummonBuff>(), (int)(value * 60), false);
			}
		}
	}
}