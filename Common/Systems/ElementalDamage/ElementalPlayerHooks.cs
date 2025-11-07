namespace PathOfTerraria.Common.Systems.ElementalDamage;

internal class ElementalPlayerHooks
{
	/// <summary>
	/// Allows a <see cref="ModPlayer"/> to run code on an elemental hit.
	/// </summary>
	public interface IElementalOnHitPlayer
	{
		void ElementalOnHitNPC(bool post, NPC target, ElementInstance ele, ElementalContainer con, ElementalContainer other, int damage, NPC.HitInfo info, Item item = null);
	}

	/// <summary>
	/// Allows a <see cref="ModPlayer"/> to run code after all elements have applied their buffs and damage.
	/// </summary>
	public interface IPostElementHitPlayer
	{
		void PostElementalHit(NPC target, ElementalContainer container, ElementalContainer other, int finalDamage, NPC.HitInfo info, Item item = null);
	}

	public static void ElementalOnHitNPC(Player player, bool post, ElementInstance ele, NPC target, ElementalContainer con, ElementalContainer other, int damage, 
		NPC.HitInfo hitInfo, Item item = null)
	{
		foreach (ModPlayer modPlayer in player.ModPlayers)
		{
			if (modPlayer is IElementalOnHitPlayer onHit)
			{
				onHit.ElementalOnHitNPC(post, target, ele, con, other, damage, hitInfo, item);
			}
		}
	}

	public static void PostElementalHit(Player player, NPC target, ElementalContainer container, ElementalContainer other, int damage, NPC.HitInfo hitInfo, Item item = null)
	{
		foreach (ModPlayer modPlayer in player.ModPlayers)
		{
			if (modPlayer is IPostElementHitPlayer onHit)
			{
				onHit.PostElementalHit(target, container, other, damage, hitInfo, item);
			}
		}
	}
}
