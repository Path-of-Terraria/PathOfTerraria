#if DEBUG
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.DropVisualization;

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

internal class DropResult(int count)
{
	public int Count = count;
	public bool IsUnique = false;
	public Dictionary<ItemRarity, int> CountsPerRarity = [];

	public void IncrementRarityCount(ItemRarity rarity)
	{
		if (!CountsPerRarity.ContainsKey(rarity))
		{
			CountsPerRarity.Add(rarity, 0);
		}

		CountsPerRarity[rarity]++;
	}
}

internal class DropTableUIState : CloseableSmartUi
{
	public static readonly Point MainPanelSize = new(920, 550);

	private enum SortMode
	{
		None,
		Count,
		Alphabetical
	}

	public override bool IsCentered => true;

	private EditableValueUI _gearRate = null;
	private EditableValueUI _currencyRate = null;
	private EditableValueUI _mapRate = null;
	private EditableValueUI _count = null;
	private EditableValueUI _level = null;
	private UIButton<string> _sortButton = null;
	private UIList _resultList = null;
	private SortMode _sort = SortMode.None;

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
		Panel.VAlign = 0.7f;

		BuildModificationPanel(Panel);
		BuildDisplay(Panel);

		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"));
		CloseButton.Left.Set(-PointsAndExitPadding - 38, 1);
		CloseButton.Top.Set(10, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) =>
		{
			Toggle();
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);

		Recalculate();
	}

	private void BuildDisplay(UICloseablePanel panel)
	{
		var bottomPanel = new UIPanel()
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixelsAndPercent(-84, 1),
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
			Height = StyleDimension.FromPixels(80),
		};

		panel.Append(topPanel);

		_gearRate = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Gear"), 0.80f, false);
		topPanel.Append(_gearRate);

		_currencyRate = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Currency"), 0.15f, false)
		{
			Left = StyleDimension.FromPixels(120)
		};
		topPanel.Append(_currencyRate);

		_mapRate = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Map"), 0.05f, false)
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

		_count = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Count"), 10f, false, 0.5, false)
		{
			Left = StyleDimension.FromPixels(474)
		};
		topPanel.Append(_count);

		_level = new(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Level"), 0.05f, false, 0.01, false)
		{
			Left = StyleDimension.FromPixels(588)
		};
		topPanel.Append(_level);

		var run = new UIButton<string>(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.Run"))
		{
			Width = StyleDimension.FromPixels(60),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(680)
		};

		run.OnLeftClick += RunDatabase;
		topPanel.Append(run);

		_sortButton = new UIButton<string>(Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.DropVisualizer.SortBy") + _sort)
		{
			Width = StyleDimension.FromPixels(110),
			Height = StyleDimension.FromPixels(60),
			Left = StyleDimension.FromPixels(744)
		};

		_sortButton.OnLeftClick += ChangeSort;
		topPanel.Append(_sortButton);
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
		for (int i = 0; i < count; ++i)
		{
			ItemDatabase.ItemRecord record = DropTable.RollMobDrops(0, 40, (float)_gearRate.Value, (float)_currencyRate.Value, (float)_mapRate.Value);

			if (record == ItemDatabase.InvalidItem)
			{
				i--;
				continue;
			}

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
	}

	private void ClickNormalize(UIMouseEvent evt, UIElement listeningElement)
	{
		double total = _gearRate.Value + _currencyRate.Value + _mapRate.Value;
		_gearRate.SetPercent(_gearRate.Value / total);
		_currencyRate.SetPercent(_currencyRate.Value / total);
		_mapRate.SetPercent(_mapRate.Value / total);
	}
}
#endif
