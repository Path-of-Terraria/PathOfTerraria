using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class GlancingBlowsPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BlockPlayer>().BlockCooldown /= 2;
		player.GetModPlayer<BlockPlayer>().AddMaxBlock(0.1f);
	}
}

internal class IncreasedBlockChancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BlockPlayer>().MultiplyBlockChance(1 + 0.1f * Level);
	}
}