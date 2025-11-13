using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class UndeadLegionMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetDamage(DamageClass.Summon) += (Value / 100f) + player.maxMinions * 0.03f;
	}
}