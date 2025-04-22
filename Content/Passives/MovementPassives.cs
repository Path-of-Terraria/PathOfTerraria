using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMoveSpeedPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.moveSpeed += 0.2f * Level;
	}
}
