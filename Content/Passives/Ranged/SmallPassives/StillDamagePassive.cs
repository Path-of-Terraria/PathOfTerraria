using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class StillDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.velocity.LengthSquared() < 0.1f)
		{
			player.GetDamage(DamageClass.Generic) += Value / 100f * Level;
		}
	}
}
