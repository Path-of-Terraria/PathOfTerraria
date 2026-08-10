using PathOfTerraria.Common.Enums;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Content.Items.Gear.Weapons;
using PathOfTerraria.Core.Items;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
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

	public TableEntryUI(int itemId, DropResult result, int count, int itemLevel)
	{
		ItemId = itemId;
		Result = result;
		Count = count;

		Width = StyleDimension.Fill;
		Height = StyleDimension.FromPixels(20);

		var text = new UIText($"[i:{ItemId}] {Lang.GetItemNameValue(ItemId)}:");
		text.Append(new UIText($"#: {Result.Count}") { Left = StyleDimension.FromPixels(230) });
		float rolledPercent = Count <= 0 ? 0f : Result.Count / (float)Count * 100f;
		text.Append(new UIText($"Roll: {rolledPercent:#0.####}%") { Left = StyleDimension.FromPixels(295) });

		if (ContentSamples.ItemsByType[ItemId].ModItem is CurrencyShard)
		{
			Append(text);
			return;
		}

		Item sample = ContentSamples.ItemsByType[ItemId];
		if (WeaponBaseTierRegistry.IsTiered(sample))
		{
			PoTStaticItemData staticData = sample.GetStaticData();
			(int Min, int Max) range = staticData.DropItemLevelRange ?? (1, int.MaxValue);
			bool eligible = staticData.CanDropAtItemLevel(itemLevel);
			var implicitAffix = WeaponImplicitFactory.Create(sample);

			text.Append(new UIText($"Gear: {Result.ExpectedGearPoolShare * 100d:#0.####}%") { Left = StyleDimension.FromPixels(390) });
			text.Append(new UIText($"T{staticData.BaseTier}") { Left = StyleDimension.FromPixels(485) });
			text.Append(new UIText($"Lv {range.Min}-{range.Max}") { Left = StyleDimension.FromPixels(525) });
			text.Append(new UIText($"x{WeaponBaseTierRegistry.GetTierWeight(staticData.BaseTier):#0.##}") { Left = StyleDimension.FromPixels(615) });

			var eligibility = new UIText(eligible ? "Eligible" : "Excluded")
			{
				Left = StyleDimension.FromPixels(675),
				TextColor = eligible ? Color.LightGreen : Color.IndianRed
			};
			text.Append(eligibility);
			text.Append(new UIText($"Implicit: {implicitAffix?.Value ?? 0f:#0.##}") { Left = StyleDimension.FromPixels(750) });
		}

		int xOff = 870;

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
				xOff += 105;
			}
		}

		Append(text);
	}
}
