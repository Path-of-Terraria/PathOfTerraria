using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class BleedTimePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BleedPlayer>().BleedTime += Value / 100f;
	}
}
