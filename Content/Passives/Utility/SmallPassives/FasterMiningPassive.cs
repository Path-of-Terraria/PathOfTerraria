using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class FasterMiningPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.pickSpeed *= MathHelper.Clamp(1f - Value / 100f, 0.1f, 1f);
	}
}
