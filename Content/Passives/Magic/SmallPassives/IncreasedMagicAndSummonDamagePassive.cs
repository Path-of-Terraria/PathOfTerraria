using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedMagicAndSummonDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Magic) += (Value / 100.0f) * Level;
		player.GetDamage(DamageClass.Summon) += (Value / 100.0f) * Level;
	}
}