using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class DamageReductionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.endurance += (Value/100.0f) * Level;
	}
}

