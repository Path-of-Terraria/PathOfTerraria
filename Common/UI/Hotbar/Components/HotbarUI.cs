using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Hotbar.Components;

/// <summary>
///		The hotbar, which may render multiple hotbar sets.
/// </summary>
internal sealed class HotbarUI : UIElement
{
	private readonly List<HotbarSetUI> sets = [];

	public void AddSet(HotbarSetUI set)
	{
		sets.Add(set);
	}
}
