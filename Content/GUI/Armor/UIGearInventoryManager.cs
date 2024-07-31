namespace PathOfTerraria.Content.GUI.Armor;

[Autoload(Side = ModSide.Client)]
public sealed class UIGearInventoryManager : ModSystem
{
	public override void OnWorldLoad()
	{
		UISystem.Enable("PoT:Inventory", "Vanilla: Inventory", 1, new UIGearInventory());
	}
}