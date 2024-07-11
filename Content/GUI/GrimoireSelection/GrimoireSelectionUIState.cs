using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.GrimoireSelection;

internal class GrimoireSelectionUIState : CloseableSmartUi
{
	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(x => x.Name == "Vanilla: Hotbar");
	}

	internal void Toggle()
	{
		RemoveAllChildren();
	}
}
