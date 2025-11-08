using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MagicFindBoost : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ItemDropModifierPlayer>().MagicFind += Value / 100f;
	}
}
