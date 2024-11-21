using PathOfTerraria.Core.UI;

namespace PathOfTerraria.Common.UI.Armor;

[Autoload(Side = ModSide.Client)]
public sealed class UIGearInventoryManager : ModSystem
{
	public const string Identifier = $"{PoTMod.ModName}:Inventory";

	public override void OnWorldLoad()
	{
		UIManager.TryEnableOrRegister(Identifier, "Vanilla: Inventory", new UIGearInventory(), 1);
		UIManager.TryRefresh(Identifier); // Re-initialize the state - redundant on first load, may want to change in the future
	}
}