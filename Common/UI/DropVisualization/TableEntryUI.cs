using PathOfTerraria.Common.Enums;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.DropVisualization;

/// <summary>
/// Represents an entry in the <see cref="DropTableUIState"/> table; an item ID, info on how often it was dropped & what rarity, and if it's unique.
/// </summary>
internal class TableEntryUI : UIElement
{
	internal readonly DropResult Result = null;
	internal readonly int ItemId = 0;

	private readonly int Count = 0;

	public TableEntryUI(int itemId, DropResult result, int count)
	{
		ItemId = itemId;
		Result = result;
		Count = count;

		Width = StyleDimension.Fill;
		Height = StyleDimension.FromPixels(20);

		var text = new UIText($"[i:{ItemId}] {Lang.GetItemNameValue(ItemId)}:");
		text.Append(new UIText($"#: {Result.Count}") { Left = StyleDimension.FromPixels(240) });
		text.Append(new UIText($"%: {Result.Count / (float)Count * 100f:#0.###}%") { Left = StyleDimension.FromPixels(320) });

		int xOff = 430;

		if (Result.IsUnique)
		{
			text.Append(new UIText(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.UniqueItem")) { Left = StyleDimension.FromPixels(xOff) });
		}
		else
		{
			for (int i = 0; i < (int)ItemRarity.Unique; ++i)
			{
				var key = (ItemRarity)i;

				if (!Result.CountsPerRarity.TryGetValue(key, out int value))
				{
					continue;
				}

				text.Append(new UIText($"{key.ToString()[..3]}: {value / (float)Result.Count * 100f:#0.#}%") { Left = StyleDimension.FromPixels(xOff) });
				xOff += 120;
			}
		}
		
		Append(text);
	}
}
