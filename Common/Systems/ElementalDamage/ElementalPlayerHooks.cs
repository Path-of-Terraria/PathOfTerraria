namespace PathOfTerraria.Common.Systems.ElementalDamage;

internal class ElementalPlayerHooks
{
	public interface IElementalOnHitPlayer
	{
		void ElementalOnHitNPC(NPC target, ElementInstance ele, ElementalContainer con, ElementalContainer other, int finalDamage, NPC.HitInfo hitInfo, Item item = null);
	}

	public static void ElementalOnHitNPC(Player player, ElementInstance ele, NPC target, ElementalContainer con, ElementalContainer other, int finalDamage, 
		NPC.HitInfo hitInfo, Item item = null)
	{
		foreach (ModPlayer modPlayer in player.ModPlayers)
		{
			if (modPlayer is IElementalOnHitPlayer onHit)
			{
				onHit.ElementalOnHitNPC(target, ele, con, other, finalDamage, hitInfo, item);
			}
		}
	}
}
