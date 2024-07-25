namespace PathOfTerraria.Helpers.Extensions;

public static class ItemExtensions
{
	public static bool IsWeapon(this Item item)
	{
		return item.damage > 0 && item.pick <= 0;
	}
}