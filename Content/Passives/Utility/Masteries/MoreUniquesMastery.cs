using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class MoreUniquesMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ItemDropModifierPlayer>().UniqueFindMultiplier += Value / 100f;
	}
}
