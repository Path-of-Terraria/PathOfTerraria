using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedRangedDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Ranged) += (Value/100.0f) * Level;
	}
}
