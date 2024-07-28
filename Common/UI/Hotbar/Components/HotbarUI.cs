using Terraria.UI;

namespace PathOfTerraria.Common.UI.Hotbar.Components;

/// <summary>
///		The hotbar, which may render multiple hotbar sets.
/// </summary>
internal sealed class HotbarUI : UIElement
{
	private readonly HotbarSetUI[] sets;

	public HotbarUI(params HotbarSetUI[] sets)
	{
		this.sets = sets;
	}
}
