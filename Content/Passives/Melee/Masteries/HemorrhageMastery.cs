using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class HemorrhageMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BleedPlayer>().MaxBleedStacks *= 2;
	}
}
