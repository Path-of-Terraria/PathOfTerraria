using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedManaRegenerationRatePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.manaRegen = (int)(player.manaRegen * (1 + (Value / 100f) * Level));
	}
}