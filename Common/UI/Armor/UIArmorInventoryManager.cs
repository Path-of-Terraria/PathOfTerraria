using PathOfTerraria.Core.UI;

namespace PathOfTerraria.Common.UI.Armor;

[Autoload(Side = ModSide.Client)]
public sealed class UIArmorInventoryManager : ModSystem
{
	public const string Identifier = $"{PoTMod.ModName}:Inventory";

	public override void OnWorldLoad()
	{
		UIManager.TryEnableOrRegister(Identifier, "Vanilla: Inventory", new UIArmorInventory(), 1);

		// Re-initialize the state - redundant on first load, may want to change in the future
		UIManager.TryRefresh(Identifier);
	}
}