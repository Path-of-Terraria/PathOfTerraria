using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

/// <summary>
/// Increases the chance of landing a critical strike by a flat amount.
/// </summary>
internal class AddedCriticalStrikeChance : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) += Value * Level;
	}
}

/// <summary>
/// Increases the chance of landing a critical strike by a percentage of the base critical strike chance.
/// </summary>
internal class IncreasedCriticalStrikeChance : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetCritChance(DamageClass.Generic) *= (1 + (Value / 100f) * Level);
	}
}

/// <summary>
/// Critical strikes cause hits to deal "extra damage" compared to a non-critical strike, the magnitude of which is determined by critical strike multiplier.
/// Increased critical strike multiplier increases the amount of extra damage dealt by critical strikes.
/// </summary>
internal class IncreasedCriticalStrikeMultiplier : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.CriticalMultiplier *= 1f + (Value / 100f) * Level;
	}
}