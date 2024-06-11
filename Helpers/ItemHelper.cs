namespace PathOfTerraria.Helpers;

internal static class ItemHelper
{
	internal static bool IsWeapon(this Item item)
	{
		return item.damage > 0 && item.pick <= 0;
	}
}