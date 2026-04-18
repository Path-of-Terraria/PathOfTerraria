#if DEBUG || STAGING
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using PathOfTerraria.Common.Systems.Affixes;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.ItemCreator;

internal class ItemCreatorPlayer : ModPlayer
{
	public static ModKeybind ToggleItemCreatorKey = null!;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		ToggleItemCreatorKey = KeybindLoader.RegisterKeybind(Mod, "ItemCreatorKey", Keys.F7);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ToggleItemCreatorKey.JustPressed)
		{
			SmartUiLoader.GetUiState<ItemCreatorUIState>().Toggle();
		}
	}
}

internal class ItemCreatorUIState : CloseableSmartUi
{
	private static readonly Point MainPanelSize = new(792, 770);

	protected override bool IsCentered => true;

	private Item _editItem = new();
	private UIImageItemSlot _itemSlot = null!;
	private UIPanel _affixEditorPanel = null!;
	private UIList? _affixList;
	private UIText _itemInfoText = null!;
	private UIText _disabledMessage = null!;
	private UIButton<string> _addAffixButton = null!;
	private UIButton<string> _clearAffixButton = null!;
	private UIElement _shardBar = null!;
	private AddAffixPanel? _addAffixOverlay;
	private bool _isRefreshing;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(x => x.Name == "Vanilla: Hotbar");
	}

	internal void Toggle()
	{
		RemoveAllChildren();
		_addAffixOverlay = null!;
		_affixList = null!;

		IsVisible = !IsVisible;

		if (!IsVisible)
		{
			return;
		}

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		Main.playerInventory = true;

		CreateMainPanel(false, MainPanelSize, false);
		BuildShardBar(Panel);
		BuildItemSection(Panel);
		BuildAffixEditor(Panel);
		AddCloseButton();
		Activate();
		Recalculate();
	}

	protected override void DefaultClose()
	{
		RemoveAllChildren();
		_addAffixOverlay = null;
		_affixList = null;
		IsVisible = false;
		SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
	}

	private void BuildShardBar(UICloseablePanel panel)
	{
		_shardBar = new UIPanel
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixels(50),
			BackgroundColor = new Color(40, 40, 60) * 0.8f,
		};
		panel.Append(_shardBar);

		int[] shardTypes = GetAllShardTypes();
		float xOffset = 4;

		foreach (int shardType in shardTypes)
		{
			var slot = new InfiniteShardSlot(shardType, () => _editItem, RefreshAffixList)
			{
				Left = StyleDimension.FromPixels(xOffset),
				VAlign = 0.5f,
			};
			_shardBar.Append(slot);
			xOffset += 40;
		}
	}

	private void BuildItemSection(UICloseablePanel panel)
	{
		var itemSection = new UIElement
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixels(96),
			Top = StyleDimension.FromPixels(56),
		};
		panel.Append(itemSection);

		Asset<Texture2D> slotBg = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/ItemSlot");

		_itemSlot = new UIImageItemSlot(
			slotBg, slotBg,
			new UIImageItemSlot.SlotWrapper(() => _editItem, v => _editItem = v),
			ItemSlot.Context.InventoryItem
		)
		{
			Left = StyleDimension.FromPixels(10),
			Top = StyleDimension.FromPixels(4),
			Predicate = (newItem, _) => newItem.IsAir || newItem.TryGetGlobalItem(out PoTGlobalItem _)
		};
		_itemSlot.OnModifyItem += (_, _, _) => OnItemChanged();
		itemSection.Append(_itemSlot);

		_addAffixButton = new UIButton<string>("Add Affix")
		{
			Width = StyleDimension.FromPixels(90),
			Height = StyleDimension.FromPixels(30),
			Left = StyleDimension.FromPixels(420),
			Top = StyleDimension.FromPixels(12),
		};
		_addAffixButton.OnLeftClick += (_, _) => ShowAddAffixPanel();
		itemSection.Append(_addAffixButton);

		_clearAffixButton = new UIButton<string>("Clear All")
		{
			Width = StyleDimension.FromPixels(80),
			Height = StyleDimension.FromPixels(30),
			Left = StyleDimension.FromPixels(516),
			Top = StyleDimension.FromPixels(12),
		};
		_clearAffixButton.OnLeftClick += (_, _) => ClearAllAffixes();
		itemSection.Append(_clearAffixButton);

		_itemInfoText = new UIText("", 0.85f)
		{
			Left = StyleDimension.FromPixels(10),
			Top = StyleDimension.FromPixels(70),
			TextColor = Color.LightGray,
		};
		itemSection.Append(_itemInfoText);

		UpdateItemInfo();
	}

	private void BuildAffixEditor(UICloseablePanel panel)
	{
		_affixEditorPanel = new UIPanel
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.FromPixelsAndPercent(-160, 1),
			Top = StyleDimension.FromPixels(160),
			BackgroundColor = new Color(30, 30, 50) * 0.6f,
		};
		panel.Append(_affixEditorPanel);

		_affixList = new UIList
		{
			Width = StyleDimension.FromPixelsAndPercent(-24, 1),
			Height = StyleDimension.Fill,
		};
		_affixEditorPanel.Append(_affixList);

		UIScrollbar scrollbar = new()
		{
			Width = StyleDimension.FromPixels(20),
			Height = StyleDimension.Fill,
			HAlign = 1f,
		};
		_affixList.SetScrollbar(scrollbar);
		_affixEditorPanel.Append(scrollbar);

		_disabledMessage = new UIText("Insert a PoT item to begin editing.", 1f)
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			TextColor = Color.Gray,
		};

		RefreshAffixList();
	}

	private void OnItemChanged()
	{
		RefreshAffixList();
		UpdateItemInfo();
	}

	private void RefreshAffixList()
	{
		if (_affixList == null || _isRefreshing)
		{
			return;
		}

		_isRefreshing = true;

		try
		{
			// Close add-affix overlay if open
			_addAffixOverlay?.Remove();
			_addAffixOverlay = null;

			_affixList.Clear();

			if (_editItem.IsAir || !_editItem.TryGetGlobalItem(out PoTGlobalItem _))
			{
				_affixEditorPanel.RemoveChild(_disabledMessage);
				_affixEditorPanel.Append(_disabledMessage);
				return;
			}

			_affixEditorPanel.RemoveChild(_disabledMessage);
			PoTInstanceItemData data = _editItem.GetInstanceData();

			for (int i = 0; i < data.Affixes.Count; i++)
			{
				var row = new AffixEditorRow(data.Affixes[i], _editItem, i, RefreshAffixList);
				_affixList.Add(row);
			}

			if (data.Affixes.Count == 0)
			{
				_affixList.Add(new UIText("No affixes. Use 'Add Affix' or apply shards.", 0.85f)
				{
					TextColor = Color.Gray,
				});
			}

			UpdateItemInfo();

			// Activate new elements so OnInitialize fires on dynamically-added children
			_affixList.Activate();
		}
		finally
		{
			_isRefreshing = false;
		}
	}

	private void UpdateItemInfo()
	{
		if (_itemInfoText == null)
		{
			return;
		}

		if (_editItem.IsAir || !_editItem.TryGetGlobalItem(out PoTGlobalItem _))
		{
			_itemInfoText.SetText("No item inserted");
			return;
		}

		PoTInstanceItemData data = _editItem.GetInstanceData();
		string info = $"Type: {data.ItemType} | Rarity: {data.Rarity} | Level: {data.RealLevel} | Affixes: {data.Affixes.Count}";

		if (data.Corrupted)
		{
			info += " | CORRUPTED";
		}

		if (data.Cloned)
		{
			info += " | CLONED";
		}

		_itemInfoText.SetText(info);
	}

	private void ShowAddAffixPanel()
	{
		if (_editItem.IsAir || !_editItem.TryGetGlobalItem(out PoTGlobalItem _))
		{
			return;
		}

		if (_addAffixOverlay != null)
		{
			_addAffixOverlay.Remove();
			_addAffixOverlay = null;
			return;
		}

		_addAffixOverlay = new AddAffixPanel(_editItem, () =>
		{
			_addAffixOverlay = null;
			RefreshAffixList();
		})
		{
			Width = StyleDimension.Fill,
			Height = StyleDimension.Fill,
		};

		_affixEditorPanel.Append(_addAffixOverlay);
		_addAffixOverlay.Activate();
		_affixEditorPanel.Recalculate();
	}

	private void ClearAllAffixes()
	{
		if (_editItem.IsAir || !_editItem.TryGetGlobalItem(out PoTGlobalItem _))
		{
			return;
		}

		PoTInstanceItemData data = _editItem.GetInstanceData();
		data.Affixes.Clear();
		data.ImplicitCount = 0;
		RefreshAffixList();
	}

	private static int[]? _shardTypes = null;
	private static int[] GetAllShardTypes()
	{
		if (_shardTypes == null)
		{
			List<int> itemIDs = new List<int>(capacity: 16);

			foreach (CurrencyShard shard in ModContent.GetContent<CurrencyShard>())
			{
				itemIDs.Add(shard.Item.type);
			}

			_shardTypes = itemIDs.ToArray();
		}

		return _shardTypes;
	}
}
#endif
