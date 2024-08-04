namespace PathOfTerraria.Common.Utilities.Extensions;

/// <summary>
///		Provides basic <see cref="Item"/> extension methods.
/// </summary>
public static class ItemExtensions
{
	/// <summary>
	///		Whether the item is a weapon or not.
	/// </summary>
	/// <param name="item">The item to check.</param>
	/// <returns><c>true</c> if the item is a weapon; otherwise, <c>false</c>.</returns>
	public static bool IsWeapon(this Item item)
	{
		return item.damage > 0 && item.pick <= 0;
	}
}