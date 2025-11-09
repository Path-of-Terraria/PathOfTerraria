namespace PathOfTerraria.Common.Systems.FullMenuPausing;

public class MenuSafeWorld : ModSystem
{
	/// <summary>
	/// Prevents the world from updating while the menu is open.
	/// </summary>
	public override void PreUpdateWorld() {
		if (!MenuSafeSystem.MenuOpen)
		{
			return;
		}

		Main.bloodMoon = false;
		Main.eclipse = false;

		if (Main.invasionType != 0) {
			Main.invasionDelay = int.MaxValue;
			Main.invasionSize = 0; 
			Main.invasionType = 0;
		}
	}
}