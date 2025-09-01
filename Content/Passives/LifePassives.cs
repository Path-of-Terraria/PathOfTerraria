using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedLifePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 20 * Level;
	}
}

internal class LifeRegenPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.lifeRegen += 2 * Level;
	}
}

internal class IncreasedPotionRecoveryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.PotionHealPower *= 1 + (Level * 10f) / 100f;
	}
}