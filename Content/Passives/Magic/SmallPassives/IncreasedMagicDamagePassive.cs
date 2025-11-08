using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMagicDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Magic) += (Value/100.0f) * Level;
	}
}
