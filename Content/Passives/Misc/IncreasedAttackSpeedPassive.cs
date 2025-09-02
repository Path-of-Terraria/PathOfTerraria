using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedAttackSpeedPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetAttackSpeed(DamageClass.Generic) *= 1 + (Value / 100f) * Level;
	}
}