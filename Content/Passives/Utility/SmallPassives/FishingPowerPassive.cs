using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class FishingPowerPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.fishingSkill += (int)Value;
	}
}
