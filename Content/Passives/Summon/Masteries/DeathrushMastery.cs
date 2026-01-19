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
				// Value is scaled by 100 in the JSON (e.g. 150 = 1.5).
				// Minion speed bonus: (Value / 10000) per minion (capped at 10 minions).
				float minions = MathF.Min(player.GetModPlayer<OldSummonSlotPlayer>().OldSlots, 10);
				projectile.GetGlobalProjectile<SpeedUpProjectile>().AddBehaviourSpeed(value / 10000f * minions);
			}

			return true;
		}
	}

	public override void BuffPlayer(Player player)
	{
		// Value is scaled by 100 in the JSON for better tunability (e.g. 150 = 1.5).
		// Player movement speed bonus: (Value / 1000) per minion (capped at 10 minions).
		player.moveSpeed += MathF.Min(player.slotsMinions, 10) * Value / 1000f;
	}
}