using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class HeartySummonsPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.statLife > player.statLifeMax2 * 0.9f)
		{
			player.GetDamage(DamageClass.Summon) += Value / 100f;
		}
	}
}