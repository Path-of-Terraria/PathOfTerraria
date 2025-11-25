using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class DeathrushMastery : Passive
{
	internal class DeathrushProjectile : GlobalProjectile
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion;
		}

		public override bool PreAI(Projectile projectile)
		{
			if (projectile.TryGetOwner(out Player player) && player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<DeathrushMastery>(out float value))
			{
				float minions = MathF.Min(player.GetModPlayer<OldSummonSlotPlayer>().OldSlots, 10);
				projectile.GetGlobalProjectile<SpeedUpProjectile>().AddBehaviourSpeed(value / 100f * minions);
			}

			return true;
		}
	}

	public override void BuffPlayer(Player player)
	{
		player.moveSpeed += MathF.Min(player.slotsMinions, 10) * Value / 100f;
	}
}