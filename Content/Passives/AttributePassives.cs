using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedIntelligencePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Intelligence += 10 * Level;
	}
}

internal class AddedStrengthPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Strength += 10 * Level;
	}
}

internal class AddedDexterityPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Dexterity += 10 * Level;
	}
}