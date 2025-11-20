using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class FasterMinionsPassive : Passive
{
	internal class FasterMinionsGlobalProjectile : GlobalProjectile
	{
		public override bool PreAI(Projectile p)
		{
			if ( p.minion && p.TryGetOwner(out Player plr) && plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<FasterMinionsPassive>(out float value))
			{
				p.GetGlobalProjectile<SpeedUpProjectile>().TotalSpeed += value / 100f;
			}

			return true;
		}
	}
}