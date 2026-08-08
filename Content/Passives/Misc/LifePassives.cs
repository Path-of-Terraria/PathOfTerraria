using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedLifePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += (int)(Value * Level);
	}
}

internal class LifeRegenPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.lifeRegen += (int)(Value * Level);
	}
}

internal class IncreasedPotionRecoveryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.PotionHealPower *= 1 + (Level * 10f) / 100f;
	}
}
