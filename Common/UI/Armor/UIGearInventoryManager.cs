using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Core.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor;

[Autoload(Side = ModSide.Client)]
public sealed class UIGearInventoryManager : ModSystem
{
	public const string Identifier = $"{PoTMod.ModName}:Inventory";
	
	public override void OnWorldLoad()
	{
		UIManager.Enable(Identifier, "Vanilla: Inventory", new UIGearInventory(), 1);
	}
}