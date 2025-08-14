using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedLifePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += Value * Level;
	}
}

internal class LifeRegenPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.lifeRegen += Value * Level;
	}
}

internal class IncreasedPotionRecoveryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.PotionHealPower += Level * (Value/100.0f);
	}
}