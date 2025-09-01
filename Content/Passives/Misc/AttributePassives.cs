using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedIntelligencePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Intelligence += Value * Level;
	}
}

internal class AddedStrengthPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Strength += Value * Level;
	}
}

internal class AddedDexterityPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Dexterity += Value * Level;
	}
}