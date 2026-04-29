using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.ItemFiltering;

internal sealed class ItemFilterUIState : UIState, IMutuallyExclusiveUI, IAutopauseUI
{
	public const string Identifier = "Item Filter UI";

	private const string Path = "Mods.PathOfTerraria.UI.ItemFilter.";

	private static readonly ItemRarity[] AllRarities = [ItemRarity.Normal, ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Unique];

	private UIList _filterList;
	private UIElement _editorPanel;
	private int _selectedIndex = -1;

	public override void OnActivate()
	{
		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<ItemFilterUIState>();

		RemoveAllChildren();

		UIPanel root = new();
		root.SetDimensions((0f, 0), (0, 0), (0, 900), (0, 560));
		root.HAlign = 0.5f;
		root.VAlign = 0.5f;
		root.OnUpdate += BlockClicks;
		Append(root);

		root.Append(new UIText(Language.GetText(Path + "Title"), 0.6f, true)
		{
			Top = StyleDimension.FromPixels(6),
			Left = StyleDimension.FromPixels(6)
		});

		var close = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")) { HAlign = 1f };
		close.SetVisibility(1, 0.6f);
		close.SetDimensions((0, 0), (0, 0), (0, 38), (0, 38));
		close.OnLeftClick += (_, _) => Toggle();
		root.Append(close);

		BuildLeftColumn(root);
		BuildRightColumn(root);

		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		if (player.Filters.Count > 0)
		{
			_selectedIndex = Math.Max(0, player.ActiveFilterIndex);
		}

		RebuildFilterList();
		RebuildEditor();
	}

	private void BuildLeftColumn(UIPanel root)
	{
		const int LeftWidth = 240;

		UIPanel column = new();
		column.SetDimensions((0, 6), (0, 50), (0, LeftWidth), (1, -94));
		root.Append(column);

		_filterList = new UIList();
		_filterList.SetDimensions((0, 0), (0, 0), (1, -24), (1, 0));
		_filterList.ListPadding = 4f;
		column.Append(_filterList);

		var bar = new UIScrollbar();
		bar.SetDimensions((0, 0), (0, 0), (0, 20), (1, 0));
		bar.HAlign = 1f;
		_filterList.SetScrollbar(bar);
		column.Append(bar);

		// Action buttons under the list. Three equal-width buttons spanning the column.
		const int ActionGap = 4;
		int actionWidth = (LeftWidth - 2 * ActionGap) / 3;

		AppendActionButton(root, Path + "New", 6 + 0 * (actionWidth + ActionGap), -50, actionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();
			player.Filters.Add(new ItemFilter { Name = Language.GetTextValue(Path + "DefaultName") });
			_selectedIndex = player.Filters.Count - 1;
			RebuildFilterList();
			RebuildEditor();
		});

		AppendActionButton(root, Path + "Duplicate", 6 + 1 * (actionWidth + ActionGap), -50, actionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (_selectedIndex < 0 || _selectedIndex >= player.Filters.Count)
			{
				return;
			}

			player.Filters.Add(player.Filters[_selectedIndex].Clone());
			_selectedIndex = player.Filters.Count - 1;
			RebuildFilterList();
			RebuildEditor();
		});

		AppendActionButton(root, Path + "Delete", 6 + 2 * (actionWidth + ActionGap), -50, actionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (_selectedIndex < 0 || _selectedIndex >= player.Filters.Count)
			{
				return;
			}

			player.Filters.RemoveAt(_selectedIndex);

			if (player.ActiveFilterIndex == _selectedIndex)
			{
				player.ActiveFilterIndex = -1;
			}
			else if (player.ActiveFilterIndex > _selectedIndex)
			{
				player.ActiveFilterIndex--;
			}

			_selectedIndex = Math.Min(_selectedIndex, player.Filters.Count - 1);
			RebuildFilterList();
			RebuildEditor();
		});
	}

	private void BuildRightColumn(UIPanel root)
	{
		_editorPanel = new UIPanel();
		_editorPanel.SetDimensions((0, 252), (0, 50), (1, -260), (1, -94));
		root.Append(_editorPanel);

		const int RightActionWidth = 140;
		const int RightActionGap = 8;

		AppendActionButton(root, Path + "SetActive", -RightActionWidth - RightActionGap - RightActionWidth - 8, -50, RightActionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (_selectedIndex < 0 || _selectedIndex >= player.Filters.Count)
			{
				return;
			}

			player.ActiveFilterIndex = _selectedIndex;
			RebuildFilterList();
		}, alignRight: true);

		AppendActionButton(root, Path + "ClearActive", -RightActionWidth - 8, -50, RightActionWidth, () =>
		{
			Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>().ActiveFilterIndex = -1;
			RebuildFilterList();
		}, alignRight: true);
	}

	private static void AppendActionButton(UIElement parent, string textKey, float leftPx, float bottomPx, int width, Action onClick, bool alignRight = false)
	{
		var button = new UIPanelButton(Language.GetText(textKey), onClick);
		button.Width = StyleDimension.FromPixels(width);
		button.Height = StyleDimension.FromPixels(36);
		button.Left = StyleDimension.FromPixels(leftPx);
		button.Top = new StyleDimension(bottomPx, 1f);

		if (alignRight)
		{
			button.HAlign = 1f;
		}

		parent.Append(button);
	}

	private void RebuildFilterList()
	{
		_filterList.Clear();

		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		for (int i = 0; i < player.Filters.Count; i++)
		{
			int index = i;
			ItemFilter filter = player.Filters[i];
			bool isActive = i == player.ActiveFilterIndex;
			bool isSelected = i == _selectedIndex;

			string label = filter.Name;
			if (isActive)
			{
				label = Language.GetTextValue(Path + "ActiveMarker", label);
			}

			var row = new UIPanel();
			row.SetPadding(0);
			row.Width = StyleDimension.FromPercent(1f);
			row.Height = StyleDimension.FromPixels(32);
			row.BackgroundColor = isSelected ? new Color(73, 94, 171) : new Color(63, 82, 151) * 0.7f;
			row.Append(new UIText(label, 0.85f) { HAlign = 0.5f, VAlign = 0.5f });
			row.OnLeftClick += (_, _) =>
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				_selectedIndex = index;
				RebuildFilterList();
				RebuildEditor();
			};
			_filterList.Add(row);
		}
	}

	private void RebuildEditor()
	{
		_editorPanel.RemoveAllChildren();

		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		if (_selectedIndex < 0 || _selectedIndex >= player.Filters.Count)
		{
			_editorPanel.Append(new UIText(Language.GetText(Path + "EmptyHint"), 0.85f)
			{
				HAlign = 0.5f,
				VAlign = 0.5f,
			});
			return;
		}

		ItemFilter filter = player.Filters[_selectedIndex];

		// Layout: each section has a label row followed by an input row.
		const int NameLabelY = 0;
		const int NameFieldY = 24;
		const int LevelLabelY = 64;
		const int LevelFieldY = 88;
		const int RarityLabelY = 128;
		const int RarityToggleY = 152;
		const int BaseTypeLabelY = 192;
		const int BaseTypeGridY = 216;
		const int FieldHeight = 28;
		const int ToggleHeight = 28;

		// Name field.
		_editorPanel.Append(new UIText(Language.GetText(Path + "Name"), 0.85f) { Top = StyleDimension.FromPixels(NameLabelY) });

		var nameField = new UIEditableText(InputType.Text, Language.GetTextValue(Path + "DefaultName"), maxChars: 40)
		{
			CurrentValue = filter.Name,
		};
		nameField.SetDimensions((0, 0), (0, NameFieldY), (1, 0), (0, FieldHeight));
		nameField.OnUpdate += _ =>
		{
			if (!string.Equals(filter.Name, nameField.CurrentValue, StringComparison.Ordinal))
			{
				filter.Name = nameField.CurrentValue;
			}
		};
		_editorPanel.Append(nameField);

		// Item level fields - labels on label row, inputs on field row below.
		_editorPanel.Append(new UIText(Language.GetText(Path + "ItemLevel"), 0.85f) { Top = StyleDimension.FromPixels(LevelLabelY) });

		var minField = new UINumberInput(Language.GetTextValue(Path + "Any"), maxChars: 4) { Min = 0 };
		minField.Value = filter.MinItemLevel;
		minField.SetDimensions((0, 0), (0, LevelFieldY), (0, 96), (0, FieldHeight));
		minField.OnValueChanged += v => filter.MinItemLevel = v;
		_editorPanel.Append(minField);

		_editorPanel.Append(new UIText("-", 0.85f)
		{
			Top = StyleDimension.FromPixels(LevelFieldY + 4),
			Left = StyleDimension.FromPixels(104),
		});

		var maxField = new UINumberInput(Language.GetTextValue(Path + "Any"), maxChars: 4) { Min = 0 };
		maxField.Value = filter.MaxItemLevel;
		maxField.SetDimensions((0, 120), (0, LevelFieldY), (0, 96), (0, FieldHeight));
		maxField.OnValueChanged += v => filter.MaxItemLevel = v;
		_editorPanel.Append(maxField);

		// Rarity toggles.
		_editorPanel.Append(new UIText(Language.GetText(Path + "Rarity"), 0.85f) { Top = StyleDimension.FromPixels(RarityLabelY) });

		const int RarityToggleWidth = 96;
		const int RarityToggleSpacing = 8;
		float rarityLeft = 0;

		foreach (ItemRarity rarity in AllRarities)
		{
			ItemRarity captured = rarity;
			var toggle = new UIToggleButton(
				Language.GetText(Path + "Rarities." + captured),
				() => filter.Rarities.Contains(captured),
				v =>
				{
					if (v)
					{
						filter.Rarities.Add(captured);
					}
					else
					{
						filter.Rarities.Remove(captured);
					}
				});
			toggle.Top = StyleDimension.FromPixels(RarityToggleY);
			toggle.Left = StyleDimension.FromPixels(rarityLeft);
			toggle.Width = StyleDimension.FromPixels(RarityToggleWidth);
			toggle.Height = StyleDimension.FromPixels(ToggleHeight);
			_editorPanel.Append(toggle);
			rarityLeft += RarityToggleWidth + RarityToggleSpacing;
		}

		// Base type toggles.
		_editorPanel.Append(new UIText(Language.GetText(Path + "BaseTypes"), 0.85f) { Top = StyleDimension.FromPixels(BaseTypeLabelY) });

		var baseTypeContainer = new UIPanel();
		baseTypeContainer.SetDimensions((0, 0), (0, BaseTypeGridY), (1, 0), (1, -BaseTypeGridY - 8));
		_editorPanel.Append(baseTypeContainer);

		var baseGrid = new UIGrid();
		baseGrid.SetDimensions((0, 0), (0, 0), (1, -24), (1, 0));
		baseGrid.ListPadding = 6f;
		baseTypeContainer.Append(baseGrid);

		var baseBar = new UIScrollbar();
		baseBar.SetDimensions((0, 0), (0, 0), (0, 20), (1, 0));
		baseBar.HAlign = 1f;
		baseGrid.SetScrollbar(baseBar);
		baseTypeContainer.Append(baseBar);

		const int BaseTypeToggleWidth = 130;

		foreach (ItemType type in GetSingleBaseTypes())
		{
			ItemType captured = type;
			var toggle = new UIToggleButton(
				LanguageHelperLocalizeBaseType(captured),
				() => (filter.BaseTypes & captured) != 0,
				v => filter.BaseTypes = v ? filter.BaseTypes | captured : filter.BaseTypes & ~captured);
			toggle.Width = StyleDimension.FromPixels(BaseTypeToggleWidth);
			toggle.Height = StyleDimension.FromPixels(ToggleHeight);
			baseGrid.Add(toggle);
		}
	}

	private static IEnumerable<ItemType> GetSingleBaseTypes()
	{
		List<ItemType> result = [];

		for (int bit = 0; bit < (int)ItemType.TypeCount; bit++)
		{
			var value = (ItemType)(1L << bit);

			if (value != ItemType.None)
			{
				result.Add(value);
			}
		}

		return result;
	}

	private static LocalizedText LanguageHelperLocalizeBaseType(ItemType type)
	{
		// Localized names for base types live under the gear localization tree.
		string key = $"Mods.{PoTMod.ModName}.Gear.{type}.Name";

		if (Language.Exists(key))
		{
			return Language.GetText(key);
		}

		return Language.GetText(Path + "BaseType.Fallback").WithFormatArgs(type.ToString());
	}

	private static void BlockClicks(UIElement affectedElement)
	{
		if (affectedElement.ContainsPoint(Main.MouseScreen))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();
	}

	public void Toggle()
	{
		SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		UIManager.TryToggleOrRegister(Identifier, "Vanilla: Mouse Text", new ItemFilterUIState(), 0, InterfaceScaleType.UI);
	}
}
