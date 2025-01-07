using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class GlancingBlowsPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BlockPlayer>().BlockCooldown /= 2;
	}
}
