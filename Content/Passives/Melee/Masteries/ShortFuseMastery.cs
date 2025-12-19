using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class ShortFuseMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<IgnitedPlayer>().IgniteDuration *= Value / 100f;
	}
}
