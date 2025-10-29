using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ArmorPenetrationPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetArmorPenetration(DamageClass.Melee) *= 1 + (Value/100.0f) * Level;
	}
}