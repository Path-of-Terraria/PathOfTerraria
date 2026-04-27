using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.DropVisualization;

#if DEBUG || STAGING
internal class DropTablePlayer : ModPlayer
{
	public static ModKeybind ToggleDropTableUIKey = null;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		ToggleDropTableUIKey = KeybindLoader.RegisterKeybind(Mod, "DropTableKey", Keys.OemBackslash);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ToggleDropTableUIKey.JustPressed)
		{
			SmartUiLoader.GetUiState<DropTableUIState>().Toggle();
		}
	}
}
#endif

internal class DropResult(int count)
{
	public int Count = count;
	public bool IsUnique = false;
	public Dictionary<ItemRarity, int> CountsPerRarity = [];

	public void IncrementRarityCount(ItemRarity rarity)
	{
		if (!CountsPerRarity.TryGetValue(rarity, out int value))
		{
			value = 0;
			CountsPerRarity.Add(rarity, value);
		}

		CountsPerRarity[rarity] = ++value;
	}
}

internal class DropTableUIState : CloseableSmartUi
{
	public static readonly Point MainPanelSize = new(1320, 900);

	private enum SortMode
	{
		None,
		Count,
		Alphabetical
	}

	private enum CategoryFilter
	{
		All,
		Gear,
		Currency,
		Maps
	}

	protected override bool IsCentered => true;

	private UIEditableValue _gearRate = null;
	private UIEditableValue _currencyRate = null;
	private UIEditableValue _mapRate = null;
	private UIEditableValue _count = null;
	private UIEditableValue _level = null;
	private UIEditableValue _rarityMod = null;
	private UIEditableValue _rateMod = null;
	private UIButton<string> _sortButton = null;
	private UIButton<string> _categoryButton = null;
	private UIPanel _categoryDropdown = null;
	private UIList _resultList = null;
	private SortMode _sort = SortMode.None;
	private CategoryFilter _categoryFilter = CategoryFilter.All;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(x => x.Name == "Vanilla: Hotbar");
	}

	internal void Toggle()
	{
		RemoveAllChildren();

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		IsVisible = !IsVisible;

		if (!IsVisible)
		{
			return;
		}

		Main.playerInventory = true;

		CreateMainPanel(false, MainPanelSize, false, true);
		Panel.VAlign = 0.5f;

		BuildModificationPanel(Panel);
		BuildDisplay(Panel);
		AddCloseButton();
		Recalculate();
	}

	private void BuildDisplay(UICloseablePanel panel)
	{
		var bottomPanel = new UIPanel()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixelsAndPercent(-184, 1),
			VAlign = 1f
		};

		panel.Append(bottomPanel);

		_resultList = new UIList
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
			VAlign = 1f,
			ManualSortMethod = SortList
		};

		bottomPanel.Append(_resultList);

		UIScrollbar bar = new()
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.Fill,
			HAlign = 1f,
		};

		_resultList.SetScrollbar(bar);
		bottomPanel.Append(bar);
	}

	private void SortList(List<UIElement> ui)
	{
		if (_sort == SortMode.None)
		{
			return;
		}

		ui.Sort(GetSort);
	}

	private int GetSort(UIElement x, UIElement y)
	{
		if (x is not TableEntryUI a || y is not TableEntryUI b)
		{
			throw new ArgumentException("Non-TableEntryUI added to table. How?");
		}

		if (_sort == SortMode.Count)
		{
			return b.Result.Count.CompareTo(a.Result.Count);
		}
		else if (_sort == SortMode.Alphabetical)
		{
			return Lang.GetItemNameValue(a.ItemId).CompareTo(Lang.GetItemNameValue(b.ItemId));
		}

		return 0;
	}

	private void BuildModificationPanel(UICloseablePanel panel)
	{
		var topPanel = new UIPanel()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixels(180),
		};

		panel.Append(topPanel);

		int defaultItemLevel = PoTMobHelper.GetAreaLevel();
		DropTable.DropCategoryWeights weights = DropTable.GetDefaultDropCategoryWeights(defaultItemLevel);

		_gearRate = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Gear"), weights.Gear, false);
		topPanel.Append(_gearRate);

		_currencyRate = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Currency"), weights.Currency, false)
		{
			Left = StyleDimension.FromPixels(120)
		};
		topPanel.Append(_currencyRate);

		_mapRate = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Map"), weights.Map, false)
		{
			Left = StyleDimension.FromPixels(256)
		};
		topPanel.Append(_mapRate);

		var normalize = new UIButton<string>(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Normalize"))
		{
			Width = StyleDimension.FromPixels(100),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(350)
		};

		normalize.OnLeftClick += ClickNormalize;
		topPanel.Append(normalize);

		_count = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Count"), 50f, false, 50, false)
		{
			Left = StyleDimension.FromPixels(464)
		};
		topPanel.Append(_count);

		_level = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Level"), defaultItemLevel / 100f, false, 0.01, false)
		{
			Left = StyleDimension.FromPixels(598)
		};
		topPanel.Append(_level);

		_rarityMod = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.RareMod"), 0, false, 0.05, false)
		{
			Left = StyleDimension.FromPixels(702)
		};
		topPanel.Append(_rarityMod);

		_rateMod = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.RateMod"), 0, false, 0.05, false)
		{
			Left = StyleDimension.FromPixels(810)
		};
		topPanel.Append(_rateMod);

		var run = new UIButton<string>(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Run"))
		{
			Width = StyleDimension.FromPixels(60),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(920)
		};

		run.OnLeftClick += RunDatabase;
		topPanel.Append(run);

		_sortButton = new UIButton<string>(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.SortBy") + _sort)
		{
			Width = StyleDimension.FromPixels(110),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(984)
		};

		_sortButton.OnLeftClick += ChangeSort;
		topPanel.Append(_sortButton);

		_categoryButton = new UIButton<string>(GetCategoryButtonText())
		{
			Width = StyleDimension.FromPixels(150),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(1108)
		};

		_categoryButton.OnLeftClick += ToggleCategoryDropdown;
		topPanel.Append(_categoryButton);
	}

	private void ChangeSort(UIMouseEvent evt, UIElement listeningElement)
	{
		_sort++;

		if (_sort > SortMode.Alphabetical)
		{
			_sort = SortMode.None;
		}

		_sortButton.SetText(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.SortBy") + _sort);
		_resultList.UpdateOrder();
	}

	private string GetCategoryButtonText()
	{
		return "Category: " + _categoryFilter;
	}

	private void ToggleCategoryDropdown(UIMouseEvent evt, UIElement listeningElement)
	{
		if (_categoryDropdown is not null)
		{
			_categoryDropdown.Remove();
			_categoryDropdown = null;
			return;
		}

		_categoryDropdown = new UIPanel
		{
			Width = StyleDimension.FromPixels(150),
			Height = StyleDimension.FromPixels(112),
			Left = StyleDimension.FromPixels(1108),
			Top = StyleDimension.FromPixels(64)
		};

		AddCategoryOption(CategoryFilter.All, 0);
		AddCategoryOption(CategoryFilter.Gear, 28);
		AddCategoryOption(CategoryFilter.Currency, 56);
		AddCategoryOption(CategoryFilter.Maps, 84);

		listeningElement.Parent.Append(_categoryDropdown);
	}

	private void AddCategoryOption(CategoryFilter filter, int top)
	{
		var button = new UIButton<string>(filter.ToString())
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixels(26),
			Top = StyleDimension.FromPixels(top)
		};

		button.OnLeftClick += (_, _) => SelectCategory(filter);
		_categoryDropdown.Append(button);
	}

	private void SelectCategory(CategoryFilter filter)
	{
		_categoryFilter = filter;
		_categoryButton.SetText(GetCategoryButtonText());
		_categoryDropdown.Remove();
		_categoryDropdown = null;
	}

	private void RunDatabase(UIMouseEvent evt, UIElement listeningElement)
	{
		_resultList.Clear();

		int count = (int)(_count.Value * 100);
		Dictionary<int, DropResult> resultsById = [];

		RollDatabase(count, resultsById);

		int check = 0;
		foreach (KeyValuePair<int, DropResult> item in resultsById)
		{
			check += item.Value.Count;
			_resultList.Add(new TableEntryUI(item.Key, item.Value, count));
		}
	}

	private void RollDatabase(int count, Dictionary<int, DropResult> resultsById)
	{
		int itemLevel = (int)Math.Round(_level.Value * 100);

		if (_categoryFilter == CategoryFilter.Currency)
		{
			RollCurrencyDatabase(count, itemLevel, resultsById);
			return;
		}

		GetCategoryRates(out float gearRate, out float currencyRate, out float mapRate);
		List<ItemDatabase.ItemRecord> items = DropTable.RollManyMobDrops(count, itemLevel, (float)_rarityMod.Value, gearRate, currencyRate, 
			mapRate, null, ItemRarity.Invalid, (float)_rateMod.Value, applyAreaLevelCategoryScaling: false);

		for (int i = 0; i < count; ++i)
		{
			ItemDatabase.ItemRecord record = items[i];

			if (record == ItemDatabase.InvalidItem)
			{
				i--;
				continue;
			}

			AddResult(resultsById, record);
		}
	}

	private void RollCurrencyDatabase(int count, int itemLevel, Dictionary<int, DropResult> resultsById)
	{
		List<ItemDatabase.ItemRecord> currency = [.. ItemDatabase.GetItemByType<CurrencyShard>()];

		for (int i = 0; i < count; ++i)
		{
			ItemDatabase.ItemRecord record = DropTable.RollList(itemLevel, (float)_rarityMod.Value, currency, _ => true, (float)_rateMod.Value);

			if (record == ItemDatabase.InvalidItem)
			{
				continue;
			}

			AddResult(resultsById, record);
		}
	}

	private static void AddResult(Dictionary<int, DropResult> resultsById, ItemDatabase.ItemRecord record)
	{
		if (!resultsById.TryGetValue(record.ItemId, out DropResult result))
		{
			resultsById.Add(record.ItemId, result = new DropResult(0));
		}

		result.Count++;

		if (record.Rarity == ItemRarity.Unique)
		{
			result.IsUnique = true;
		}
		else
		{
			result.IncrementRarityCount(record.Rarity);
		}
	}

	private void GetCategoryRates(out float gearRate, out float currencyRate, out float mapRate)
	{
		gearRate = (float)_gearRate.Value;
		currencyRate = (float)_currencyRate.Value;
		mapRate = (float)_mapRate.Value;

		switch (_categoryFilter)
		{
			case CategoryFilter.Gear:
				currencyRate = 0;
				mapRate = 0;
				break;
			case CategoryFilter.Currency:
				gearRate = 0;
				mapRate = 0;
				break;
			case CategoryFilter.Maps:
				gearRate = 0;
				currencyRate = 0;
				break;
		}
	}

	private void ClickNormalize(UIMouseEvent evt, UIElement listeningElement)
	{
		double total = _gearRate.Value + _currencyRate.Value + _mapRate.Value;
		_gearRate.SetPercent(_gearRate.Value / total);
		_currencyRate.SetPercent(_currencyRate.Value / total);
		_mapRate.SetPercent(_mapRate.Value / total);
	}
}
