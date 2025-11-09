namespace PathOfTerraria.Common.Systems.FullMenuPausing;

public class MenuSafeSystem : ModSystem
{
	/// <summary>
	/// Toggle true when the menu is open. This is used to prevent certain things from happening.
	/// </summary>
	public static bool MenuOpen;
}