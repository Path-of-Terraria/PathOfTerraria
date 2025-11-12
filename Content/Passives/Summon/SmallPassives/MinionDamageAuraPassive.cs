using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MinionDamageAuraPassive : Passive
{
	public class MinionDamageAuraProjectile : GlobalProjectile
	{
		public override void AI(Projectile projectile)
		{
			foreach (Player player in Main.ActivePlayers)
			{

			}
		}
	}
}