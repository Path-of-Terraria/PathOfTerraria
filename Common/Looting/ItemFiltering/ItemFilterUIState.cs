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
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.ItemFiltering;

internal sealed class ItemFilterUIState : UIState, IMutuallyExclusiveUI, IAutopauseUI
{
	public const string Identifier = "Item Filter UI";

	private const string Path = "Mods.PathOfTerraria.UI.ItemFilter.";

	private static readonly ItemRarity[] AllRarities = [ItemRarity.Normal, ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Unique];
	private static readonly ItemType[] AllBaseTypes = CreateSingleBaseTypes();
	private static readonly ItemType AllBaseTypesMask = ItemType.All;

	private UIList _filterList;
	private UIList _ruleList;
	private UIElement _editorPanel;
	private int _selectedFilterIndex = -1;
	private int _selectedRuleIndex = -1;

	public override void OnActivate()
	{
		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<ItemFilterUIState>();

		RemoveAllChildren();

		UIPanel root = new();
		root.SetDimensions((0f, 0), (0, 0), (0, 1280), (0, 740));
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

		BuildFilterColumn(root);
		BuildRuleColumn(root);
		BuildEditorColumn(root);

		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		if (player.Filters.Count > 0)
		{
			_selectedFilterIndex = player.ActiveFilterIndex >= 0 && player.ActiveFilterIndex < player.Filters.Count
				? player.ActiveFilterIndex
				: 0;
		}

		EnsureSelectedRule();
		RebuildFilterList();
		RebuildRuleList();
		RebuildEditor();
	}

	private void BuildFilterColumn(UIPanel root)
	{
		const int Left = 6;
		const int Top = 50;
		const int Width = 260;

		root.Append(new UIText(Language.GetText(Path + "Filters"), 0.85f) { Left = StyleDimension.FromPixels(Left), Top = StyleDimension.FromPixels(Top - 28) });

		UIPanel column = new();
		column.SetDimensions((0, Left), (0, Top), (0, Width), (1, -94));
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

		const int ActionGap = 4;
		int actionWidth = (Width - 2 * ActionGap) / 3;

		AppendActionButton(root, Path + "New", Left + 0 * (actionWidth + ActionGap), -50, actionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();
			var filter = new ItemFilter { Name = Language.GetTextValue(Path + "DefaultName") };
			filter.Rules.Add(CreateDefaultRule(1));
			player.Filters.Add(filter);
			_selectedFilterIndex = player.Filters.Count - 1;
			_selectedRuleIndex = 0;
			player.ActiveFilterIndex = _selectedFilterIndex;
			RebuildFilterList();
			RebuildRuleList();
			RebuildEditor();
		});

		AppendActionButton(root, Path + "Duplicate", Left + 1 * (actionWidth + ActionGap), -50, actionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (!TryGetSelectedFilter(player, out ItemFilter filter))
			{
				return;
			}

			player.Filters.Add(filter.Clone());
			_selectedFilterIndex = player.Filters.Count - 1;
			_selectedRuleIndex = player.Filters[_selectedFilterIndex].Rules.Count > 0 ? 0 : -1;
			RebuildFilterList();
			RebuildRuleList();
			RebuildEditor();
		});

		AppendActionButton(root, Path + "Delete", Left + 2 * (actionWidth + ActionGap), -50, actionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (_selectedFilterIndex < 0 || _selectedFilterIndex >= player.Filters.Count)
			{
				return;
			}

			player.Filters.RemoveAt(_selectedFilterIndex);

			if (player.ActiveFilterIndex == _selectedFilterIndex)
			{
				player.ActiveFilterIndex = -1;
			}
			else if (player.ActiveFilterIndex > _selectedFilterIndex)
			{
				player.ActiveFilterIndex--;
			}

			_selectedFilterIndex = Math.Min(_selectedFilterIndex, player.Filters.Count - 1);
			EnsureSelectedRule();
			RebuildFilterList();
			RebuildRuleList();
			RebuildEditor();
		});
	}

	private void BuildRuleColumn(UIPanel root)
	{
		const int Left = 280;
		const int Top = 50;
		const int Width = 330;

		root.Append(new UIText(Language.GetText(Path + "Rules"), 0.85f) { Left = StyleDimension.FromPixels(Left), Top = StyleDimension.FromPixels(Top - 28) });

		UIPanel column = new();
		column.SetDimensions((0, Left), (0, Top), (0, Width), (1, -128));
		root.Append(column);

		_ruleList = new UIList();
		_ruleList.SetDimensions((0, 0), (0, 0), (1, -24), (1, 0));
		_ruleList.ListPadding = 4f;
		column.Append(_ruleList);

		var bar = new UIScrollbar();
		bar.SetDimensions((0, 0), (0, 0), (0, 20), (1, 0));
		bar.HAlign = 1f;
		_ruleList.SetScrollbar(bar);
		column.Append(bar);

		const int ActionGap = 4;
		int wideActionWidth = (Width - 2 * ActionGap) / 3;
		int narrowActionWidth = (Width - ActionGap) / 2;

		AppendActionButton(root, Path + "AddRule", Left + 0 * (wideActionWidth + ActionGap), -84, wideActionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (!TryGetSelectedFilter(player, out ItemFilter filter))
			{
				return;
			}

			filter.Rules.Add(CreateDefaultRule(filter.Rules.Count + 1));
			_selectedRuleIndex = filter.Rules.Count - 1;
			RebuildRuleList();
			RebuildEditor();
		});

		AppendActionButton(root, Path + "DuplicateRule", Left + 1 * (wideActionWidth + ActionGap), -84, wideActionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (!TryGetSelectedRule(player, out ItemFilter filter, out ItemFilterRule rule))
			{
				return;
			}

			filter.Rules.Insert(_selectedRuleIndex + 1, rule.Clone());
			_selectedRuleIndex++;
			RebuildRuleList();
			RebuildEditor();
		});

		AppendActionButton(root, Path + "DeleteRule", Left + 2 * (wideActionWidth + ActionGap), -84, wideActionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (!TryGetSelectedRule(player, out ItemFilter filter, out _))
			{
				return;
			}

			filter.Rules.RemoveAt(_selectedRuleIndex);
			_selectedRuleIndex = Math.Min(_selectedRuleIndex, filter.Rules.Count - 1);
			RebuildRuleList();
			RebuildEditor();
		});

		AppendActionButton(root, Path + "MoveUp", Left, -44, narrowActionWidth, () => MoveSelectedRule(-1));
		AppendActionButton(root, Path + "MoveDown", Left + narrowActionWidth + ActionGap, -44, narrowActionWidth, () => MoveSelectedRule(1));
	}

	private void BuildEditorColumn(UIPanel root)
	{
		_editorPanel = new UIPanel();
		_editorPanel.SetDimensions((0, 626), (0, 50), (1, -634), (1, -94));
		root.Append(_editorPanel);

		const int RightActionWidth = 140;
		const int RightActionGap = 8;

		AppendActionButton(root, Path + "SetActive", -RightActionWidth - RightActionGap - RightActionWidth - 8, -50, RightActionWidth, () =>
		{
			ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

			if (_selectedFilterIndex < 0 || _selectedFilterIndex >= player.Filters.Count)
			{
				return;
			}

			player.ActiveFilterIndex = _selectedFilterIndex;
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
		var button = new UIButton<LocalizedText>(Language.GetText(textKey))
		{
			Width = StyleDimension.FromPixels(width),
			Height = StyleDimension.FromPixels(36),
			Left = StyleDimension.FromPixels(leftPx),
			Top = new StyleDimension(bottomPx, 1f),
		};

		if (alignRight)
		{
			button.HAlign = 1f;
		}

		button.OnLeftClick += (_, _) =>
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
			onClick();
		};

		parent.Append(button);
	}

	private static void AppendEditorButton(UIElement parent, string textKey, float leftPx, float topPx, int width, Action onClick)
	{
		var button = new UIButton<LocalizedText>(Language.GetText(textKey))
		{
			Width = StyleDimension.FromPixels(width),
			Height = StyleDimension.FromPixels(28),
			Left = StyleDimension.FromPixels(leftPx),
			Top = StyleDimension.FromPixels(topPx),
		};

		button.OnLeftClick += (_, _) =>
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
			onClick();
		};

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
			bool isSelected = i == _selectedFilterIndex;

			string label = filter.Name;
			if (isActive)
			{
				label = Language.GetTextValue(Path + "ActiveMarker", label);
			}

			var row = new UIPanel();
			row.SetPadding(0);
			row.Width = StyleDimension.FromPercent(1f);
			row.Height = StyleDimension.FromPixels(34);
			row.BackgroundColor = isSelected ? new Color(73, 94, 171) : new Color(63, 82, 151) * 0.7f;
			row.Append(new UIText(label, 0.75f) { HAlign = 0.5f, VAlign = 0.5f });
			row.OnLeftClick += (_, _) =>
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				_selectedFilterIndex = index;
				EnsureSelectedRule();
				RebuildFilterList();
				RebuildRuleList();
				RebuildEditor();
			};
			_filterList.Add(row);
		}
	}

	private void RebuildRuleList()
	{
		_ruleList.Clear();

		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		if (!TryGetSelectedFilter(player, out ItemFilter filter))
		{
			return;
		}

		for (int i = 0; i < filter.Rules.Count; i++)
		{
			int index = i;
			ItemFilterRule rule = filter.Rules[i];
			bool isSelected = i == _selectedRuleIndex;
			string action = Language.GetTextValue(Path + "Actions." + rule.Action);
			string label = Language.GetTextValue(Path + "RuleSummary", i + 1, action, rule.Name);

			var row = new UIPanel();
			row.SetPadding(0);
			row.Width = StyleDimension.FromPercent(1f);
			row.Height = StyleDimension.FromPixels(34);
			row.BackgroundColor = isSelected ? new Color(73, 94, 171) : GetRuleColor(rule.Action) * 0.75f;
			row.Append(new UIText(label, 0.72f) { HAlign = 0.5f, VAlign = 0.5f });
			row.OnLeftClick += (_, _) =>
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				_selectedRuleIndex = index;
				RebuildRuleList();
				RebuildEditor();
			};
			_ruleList.Add(row);
		}
	}

	private void RebuildEditor()
	{
		_editorPanel.RemoveAllChildren();

		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		if (!TryGetSelectedFilter(player, out ItemFilter filter))
		{
			_editorPanel.Append(new UIText(Language.GetText(Path + "EmptyHint"), 0.85f)
			{
				HAlign = 0.5f,
				VAlign = 0.5f,
			});
			return;
		}

		const int FieldHeight = 28;
		const int ToggleHeight = 28;

		_editorPanel.Append(new UIText(Language.GetText(Path + "FilterName"), 0.85f) { Top = StyleDimension.FromPixels(0) });

		var filterNameField = new UIEditableText(InputType.Text, Language.GetTextValue(Path + "DefaultName"), maxChars: 40)
		{
			CurrentValue = filter.Name,
		};
		filterNameField.SetDimensions((0, 0), (0, 24), (1, 0), (0, FieldHeight));
		filterNameField.OnUpdate += _ =>
		{
			if (!string.Equals(filter.Name, filterNameField.CurrentValue, StringComparison.Ordinal))
			{
				filter.Name = filterNameField.CurrentValue;
				RebuildFilterList();
			}
		};
		_editorPanel.Append(filterNameField);

		if (!TryGetSelectedRule(player, out _, out ItemFilterRule rule))
		{
			_editorPanel.Append(new UIText(Language.GetText(Path + "RuleEmptyHint"), 0.85f)
			{
				HAlign = 0.5f,
				VAlign = 0.5f,
			});
			return;
		}

		const int RuleNameLabelY = 64;
		const int RuleNameFieldY = 88;
		const int ActionLabelY = 128;
		const int ActionToggleY = 152;
		const int LevelLabelY = 192;
		const int LevelFieldY = 216;
		const int RarityLabelY = 256;
		const int RarityToggleY = 280;
		const int BaseTypeLabelY = 320;
		const int BaseTypeBulkY = 344;
		const int BaseTypeGridY = 380;

		_editorPanel.Append(new UIText(Language.GetText(Path + "RuleName"), 0.85f) { Top = StyleDimension.FromPixels(RuleNameLabelY) });

		var ruleNameField = new UIEditableText(InputType.Text, Language.GetTextValue(Path + "DefaultRuleName", _selectedRuleIndex + 1), maxChars: 40)
		{
			CurrentValue = rule.Name,
		};
		ruleNameField.SetDimensions((0, 0), (0, RuleNameFieldY), (1, 0), (0, FieldHeight));
		ruleNameField.OnUpdate += _ =>
		{
			if (!string.Equals(rule.Name, ruleNameField.CurrentValue, StringComparison.Ordinal))
			{
				rule.Name = ruleNameField.CurrentValue;
				RebuildRuleList();
			}
		};
		_editorPanel.Append(ruleNameField);

		_editorPanel.Append(new UIText(Language.GetText(Path + "RuleAction"), 0.85f) { Top = StyleDimension.FromPixels(ActionLabelY) });
		AppendRuleActionToggle(rule, ItemFilterRuleAction.Show, 0, ActionToggleY);
		AppendRuleActionToggle(rule, ItemFilterRuleAction.Hide, 112, ActionToggleY);
		AppendRuleActionToggle(rule, ItemFilterRuleAction.Highlight, 224, ActionToggleY);

		_editorPanel.Append(new UIText(Language.GetText(Path + "ItemLevel"), 0.85f) { Top = StyleDimension.FromPixels(LevelLabelY) });

		var minField = new UINumberInput(Language.GetTextValue(Path + "Any"), maxChars: 4) { Min = 0 };
		minField.Value = rule.MinItemLevel;
		minField.SetDimensions((0, 0), (0, LevelFieldY), (0, 96), (0, FieldHeight));
		minField.OnValueChanged += v => rule.MinItemLevel = v;
		_editorPanel.Append(minField);

		_editorPanel.Append(new UIText("-", 0.85f)
		{
			Top = StyleDimension.FromPixels(LevelFieldY + 4),
			Left = StyleDimension.FromPixels(104),
		});

		var maxField = new UINumberInput(Language.GetTextValue(Path + "Any"), maxChars: 4) { Min = 0 };
		maxField.Value = rule.MaxItemLevel;
		maxField.SetDimensions((0, 120), (0, LevelFieldY), (0, 96), (0, FieldHeight));
		maxField.OnValueChanged += v => rule.MaxItemLevel = v;
		_editorPanel.Append(maxField);

		_editorPanel.Append(new UIText(Language.GetText(Path + "Rarity"), 0.85f) { Top = StyleDimension.FromPixels(RarityLabelY) });

		const int RarityToggleWidth = 96;
		const int RarityToggleSpacing = 8;
		float rarityLeft = 0;

		foreach (ItemRarity rarity in AllRarities)
		{
			ItemRarity captured = rarity;
			var toggle = new UIToggleButton(
				Language.GetText(Path + "Rarities." + captured),
				() => rule.Rarities.Contains(captured),
				v =>
				{
					if (v)
					{
						rule.Rarities.Add(captured);
					}
					else
					{
						rule.Rarities.Remove(captured);
					}
				});
			toggle.Top = StyleDimension.FromPixels(RarityToggleY);
			toggle.Left = StyleDimension.FromPixels(rarityLeft);
			toggle.Width = StyleDimension.FromPixels(RarityToggleWidth);
			toggle.Height = StyleDimension.FromPixels(ToggleHeight);
			_editorPanel.Append(toggle);
			rarityLeft += RarityToggleWidth + RarityToggleSpacing;
		}

		_editorPanel.Append(new UIText(Language.GetText(Path + "BaseTypes"), 0.85f) { Top = StyleDimension.FromPixels(BaseTypeLabelY) });
		AppendEditorButton(_editorPanel, Path + "SelectAllBaseTypes", 0, BaseTypeBulkY, 116, () =>
		{
			rule.BaseTypes = AllBaseTypesMask;
			RebuildEditor();
		});
		AppendEditorButton(_editorPanel, Path + "DeselectAllBaseTypes", 124, BaseTypeBulkY, 132, () =>
		{
			rule.BaseTypes = ItemType.None;
			RebuildEditor();
		});

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

		foreach (ItemType type in AllBaseTypes)
		{
			ItemType captured = type;
			var toggle = new UIToggleButton(
				LanguageHelperLocalizeBaseType(captured),
				() => (rule.BaseTypes & captured) != 0,
				v => rule.BaseTypes = v ? rule.BaseTypes | captured : rule.BaseTypes & ~captured);
			toggle.Width = StyleDimension.FromPixels(BaseTypeToggleWidth);
			toggle.Height = StyleDimension.FromPixels(ToggleHeight);
			baseGrid.Add(toggle);
		}
	}

	private void AppendRuleActionToggle(ItemFilterRule rule, ItemFilterRuleAction action, float left, float top)
	{
		var toggle = new UIToggleButton(
			Language.GetText(Path + "Actions." + action),
			() => rule.Action == action,
			v =>
			{
				if (!v)
				{
					return;
				}

				rule.Action = action;
				RebuildRuleList();
			},
			0.8f);
		toggle.Top = StyleDimension.FromPixels(top);
		toggle.Left = StyleDimension.FromPixels(left);
		toggle.Width = StyleDimension.FromPixels(104);
		toggle.Height = StyleDimension.FromPixels(28);
		_editorPanel.Append(toggle);
	}

	private static ItemFilterRule CreateDefaultRule(int ordinal)
	{
		return new ItemFilterRule
		{
			Name = Language.GetTextValue(Path + "DefaultRuleName", ordinal),
			Action = ItemFilterRuleAction.Show
		};
	}

	private void MoveSelectedRule(int direction)
	{
		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		if (!TryGetSelectedRule(player, out ItemFilter filter, out _))
		{
			return;
		}

		int newIndex = _selectedRuleIndex + direction;

		if (newIndex < 0 || newIndex >= filter.Rules.Count)
		{
			return;
		}

		(filter.Rules[_selectedRuleIndex], filter.Rules[newIndex]) = (filter.Rules[newIndex], filter.Rules[_selectedRuleIndex]);
		_selectedRuleIndex = newIndex;
		RebuildRuleList();
		RebuildEditor();
	}

	private void EnsureSelectedRule()
	{
		ItemFilterPlayer player = Main.LocalPlayer.GetModPlayer<ItemFilterPlayer>();

		if (!TryGetSelectedFilter(player, out ItemFilter filter) || filter.Rules.Count == 0)
		{
			_selectedRuleIndex = -1;
			return;
		}

		if (_selectedRuleIndex < 0 || _selectedRuleIndex >= filter.Rules.Count)
		{
			_selectedRuleIndex = 0;
		}
	}

	private bool TryGetSelectedFilter(ItemFilterPlayer player, out ItemFilter filter)
	{
		if (_selectedFilterIndex >= 0 && _selectedFilterIndex < player.Filters.Count)
		{
			filter = player.Filters[_selectedFilterIndex];
			return true;
		}

		filter = null;
		return false;
	}

	private bool TryGetSelectedRule(ItemFilterPlayer player, out ItemFilter filter, out ItemFilterRule rule)
	{
		if (TryGetSelectedFilter(player, out filter) && _selectedRuleIndex >= 0 && _selectedRuleIndex < filter.Rules.Count)
		{
			rule = filter.Rules[_selectedRuleIndex];
			return true;
		}

		rule = null;
		return false;
	}

	private static Color GetRuleColor(ItemFilterRuleAction action)
	{
		return action == ItemFilterRuleAction.Show
			? new Color(78, 122, 72)
			: action == ItemFilterRuleAction.Highlight
				? new Color(120, 108, 47)
				: new Color(122, 70, 70);
	}

	private static ItemType[] CreateSingleBaseTypes()
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

		return [.. result];
	}

	private static LocalizedText LanguageHelperLocalizeBaseType(ItemType type)
	{
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
