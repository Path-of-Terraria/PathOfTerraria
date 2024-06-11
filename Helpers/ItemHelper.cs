namespace PathOfTerraria.Helpers;

internal static class ItemHelper
{
	internal static bool IsWeapon(this Item item) => item.damage > 0 && item.pick <= 0;
}