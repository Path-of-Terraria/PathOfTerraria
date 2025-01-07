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

internal class IncreasedBlockChancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BlockPlayer>().MultiplyBlockChance(0.1f * Level);
	}
}