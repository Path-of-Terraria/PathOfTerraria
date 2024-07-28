using Terraria.UI;

namespace PathOfTerraria.Common.UI.Hotbar.Components;

internal sealed class SelectedHotbarSlot : UIElement
{
	public HotbarSlot? SelectedSlot { get; set; }

	private int animation;
	private float selectorX;
	private float selectorTarget;
}
