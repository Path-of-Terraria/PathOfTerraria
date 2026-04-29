using PathOfTerraria.Common.Looting.ItemFiltering;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.VirtualBagUI;

internal class VirtualBagUIState : UIState, IMutuallyExclusiveUI, IAutopauseUI
{
	public const string Identifier = "Virtual Bag UI";

	private const string MatchedTab = "matched";
	private const string FilteredOutTab = "filteredOut";

	private UIGrid _storageGrid;
	private string _activeTab = MatchedTab;
	private UIPanelTab _matchedTab;
	private UIPanelTab _filteredOutTab;
	private UIText _filterLabel;

	public override void OnActivate()
	{
		const string TexturePath = "PathOfTerraria/Assets/UI/SquarePanel";
		const int GridBuffer = 50;
		const string Path = "Mods.PathOfTerraria.UI.VirtualBag.";

		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<VirtualBagUIState>();

		UIPanel panel = new();
		panel.SetDimensions((0f, 0), (0, 0), (0, 800), (0, 500));
		panel.VAlign = 0.5f;
		panel.HAlign = 0.5f;
		panel.OnUpdate += BlockClicks;
		Append(panel);

		panel.Append(new UIText(Language.GetText(Path + "Title"), 0.6f, true) { Top = StyleDimension.FromPixels(6), Left = StyleDimension.FromPixels(6) });

		string desc = Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().ConfirmedExit ? "Review" : "Description";
		panel.Append(new UIText(Language.GetText(Path + desc), 0.9f) { HAlign = 1f, Left = StyleDimension.FromPixels(-50), Top = StyleDimension.FromPixels(6) });

		var close = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")) { HAlign = 1f };
		close.SetVisibility(1, 0.6f);
		close.SetDimensions((0, 0), (0, 0), (0, 38), (0, 38));
		close.OnLeftClick += Close;
		panel.Append(close);

		const int TabBarHeight = 32;
		const int FilterLabelHeight = 22;

		_matchedTab = new UIPanelTab(MatchedTab, Language.GetText(Path + "TabMatched"), 0.85f);
		_matchedTab.SetPadding(6);
		_matchedTab.Top = StyleDimension.FromPixels(GridBuffer);
		_matchedTab.Left = StyleDimension.FromPixels(0);
		_matchedTab.BackgroundColor.A = 255;
		_matchedTab.OnLeftClick += (_, _) => SetActiveTab(MatchedTab);
		panel.Append(_matchedTab);

		_filteredOutTab = new UIPanelTab(FilteredOutTab, Language.GetText(Path + "TabFilteredOut"), 0.85f);
		_filteredOutTab.SetPadding(6);
		_filteredOutTab.Top = StyleDimension.FromPixels(GridBuffer);
		_filteredOutTab.BackgroundColor.A = 255;
		_filteredOutTab.OnLeftClick += (_, _) => SetActiveTab(FilteredOutTab);
		panel.Append(_filteredOutTab);

		// Position the second tab after the first one once it has measured itself.
		_matchedTab.Recalculate();
		_filteredOutTab.Left = StyleDimension.FromPixels(_matchedTab.GetDimensions().Width + 8);
		_filteredOutTab.Recalculate();

		_filterLabel = new UIText(BuildFilterLabel(), 0.85f)
		{
			Top = StyleDimension.FromPixels(GridBuffer + 4),
			HAlign = 1f,
			Left = StyleDimension.FromPixels(-8),
		};
		panel.Append(_filterLabel);

		UIPanel gridPanel = new(ModContent.Request<Texture2D>(TexturePath + "Background"), ModContent.Request<Texture2D>(TexturePath + "Outline"));
		gridPanel.SetDimensions((0, 0), (0, GridBuffer + TabBarHeight + FilterLabelHeight), (1, -30), (1, -(GridBuffer + TabBarHeight + FilterLabelHeight)));
		panel.Append(gridPanel);

		_storageGrid = new UIGrid();
		_storageGrid.SetDimensions((0, 0), (0, 0), (1, 0), (1, 0));

		_storageGrid.ManualSortMethod = (list) => list.Sort((x, y) => // Manual sort fixes items shuffling almost completely every time the UI is opened, and also gives a consistent order
		{
			if (x is not UIItemIcon xItem)
			{
				return -1;
			}
			if (y is not UIItemIcon yItem)
			{
				return 1;
			}

			Item itemX = GetItem(xItem);
			Item itemY = GetItem(yItem);
			return itemX.type.CompareTo(itemY.type);
		});

		gridPanel.Append(_storageGrid);

		int gridTopOffset = GridBuffer + TabBarHeight + FilterLabelHeight;

		UIScrollbar bar = new();
		bar.SetDimensions((0, 0), (0, gridTopOffset), (0, 20), (1, -gridTopOffset));
		bar.HAlign = 1f;
		_storageGrid.SetScrollbar(bar);
		panel.Append(bar);

		SetActiveTab(_activeTab);
		RefreshStorage();

		if (Main.LocalPlayer.TryGetModPlayer(out VirtualBagStoragePlayer plr))
		{
			UIElement summaryPanel = BuildLootSummaryPanel(plr, Path);

			if (summaryPanel is not null)
			{
				panel.Append(summaryPanel);
			}
		}
	}

	private static UIPanel BuildLootSummaryPanel(VirtualBagStoragePlayer plr, string localizationPath)
	{
		List<UIElement> elements = [];
		int topOffset = 0;

		AddLootSection(elements, plr.ConfluxResourcesCollected, localizationPath + "Conflux", localizationPath + "ConfluxTooltip", ref topOffset);
		AddLootSection(elements, plr.CurrencyShardsCollected, localizationPath + "CurrencyShards", localizationPath + "CurrencyShardsTooltip", ref topOffset);

		if (elements.Count == 0)
		{
			return null;
		}

		UIPanel lootPanel = new()
		{
			HAlign = 1,
			Left = StyleDimension.FromPixels(148),
			Width = StyleDimension.FromPixels(132),
			Height = StyleDimension.FromPixels(topOffset + 16),
			Top = StyleDimension.FromPixels(-12)
		};

		foreach (UIElement element in elements)
		{
			lootPanel.Append(element);
		}

		return lootPanel;
	}

	private static void AddLootSection(List<UIElement> elements, Dictionary<int, int> loot, string titleKey, string tooltipKey, ref int topOffset)
	{
		if (loot.Count == 0)
		{
			return;
		}

		if (elements.Count > 0)
		{
			topOffset += 8;
		}

		var textUI = new UIText(Language.GetText(titleKey), 1.1f)
		{
			Top = StyleDimension.FromPixels(topOffset)
		};

		textUI.OnUpdate += (self) =>
		{
			if (self.ContainsPoint(Main.MouseScreen))
			{
				Tooltip.Create(new TooltipDescription()
				{
					Identifier = tooltipKey,
					SimpleSubtitle = Language.GetTextValue(tooltipKey)
				});
			}
		};

		elements.Add(textUI);
		topOffset += 24;

		elements.Add(new UIImageFramed(TextureAssets.MagicPixel, new Rectangle(0, 0, 70, 2))
		{
			Top = StyleDimension.FromPixels(topOffset - 4)
		});

		foreach (KeyValuePair<int, int> entry in loot.OrderBy(x => x.Key))
		{
			if (!ContentSamples.ItemsByType.ContainsKey(entry.Key))
			{
				continue;
			}

			elements.Add(new UIText($"[i:{entry.Key}]: {entry.Value}x")
			{
				HAlign = 0,
				VAlign = 0,
				Top = StyleDimension.FromPixels(topOffset)
			});

			topOffset += 28;
		}
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_item")]
	public static extern ref Item GetItem(UIItemIcon icon);

	private void Close(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		UIManager.TryToggleOrRegister(Identifier, "Vanilla: Mouse Text", new VirtualBagUIState(), 0, InterfaceScaleType.UI);
	}

	private void BlockClicks(UIElement affectedElement)
	{
		if (affectedElement.ContainsPoint(Main.MouseScreen))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();

		if (Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().ConfirmedExit)
		{
			SubworldSystem.Exit();
		}
	}

	private void SetActiveTab(string tab)
	{
		_activeTab = tab;

		if (_matchedTab is not null)
		{
			_matchedTab.TextColor = tab == MatchedTab ? Color.Yellow : Color.White;
		}

		if (_filteredOutTab is not null)
		{
			_filteredOutTab.TextColor = tab == FilteredOutTab ? Color.Yellow : Color.White;
		}

		SoundEngine.PlaySound(SoundID.MenuTick);
		RefreshStorage();
	}

	private static LocalizedText BuildFilterLabel()
	{
		const string Path = "Mods.PathOfTerraria.UI.VirtualBag.";

		string filterName = Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().LastFilterName;

		return string.IsNullOrEmpty(filterName)
			? Language.GetText(Path + "NoFilter")
			: Language.GetText(Path + "FilterLabel").WithFormatArgs(filterName);
	}

	private List<Item> GetActiveStorage(VirtualBagStoragePlayer player)
	{
		return _activeTab == FilteredOutTab ? player.FilteredOutStorage : player.MatchedStorage;
	}

	internal void RefreshStorage()
	{
		_storageGrid.Clear();

		VirtualBagStoragePlayer player = Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>();

		if (_filterLabel is not null)
		{
			_filterLabel.SetText(BuildFilterLabel());
		}

		foreach (Item item in GetActiveStorage(player))
		{
			var storageIcon = new UIItemIcon(item, false)
			{
				Width = StyleDimension.FromPixels(36),
				Height = StyleDimension.FromPixels(36)
			};
			_storageGrid.Add(storageIcon);

			var src = new Rectangle(20 * (int)item.GetInstanceData().Rarity, 0, 18, 18);
			var itemRarity = new UIImageFramed(ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/ItemRarityIcons"), src) 
			{ 
				Left = StyleDimension.FromPixels(16),
				Top = StyleDimension.FromPixels(20)
			};
			
			storageIcon.Append(itemRarity);

			storageIcon.OnRightClick += (_, self) => AddItemToInv(item);
			storageIcon.OnUpdate += (self) =>
			{
				if (self.ContainsPoint(Main.MouseScreen))
				{
					UpdateSlot(item);
				}
			};
		}

		_storageGrid.Recalculate();
		_storageGrid.Recalculate();
	}

	private void AddItemToInv(Item item)
	{
		Player plr = Main.LocalPlayer;
		bool hasModified = false;

		for (int i = 0; i < Main.InventoryItemSlotsCount; i++)
		{
			ref Item invItem = ref plr.inventory[i];

			if (invItem.IsAir)
			{
				invItem = item.Clone();
				hasModified = true;
				break;
			}
			else if (invItem.type == item.type && invItem.stack < invItem.maxStack)
			{
				int newStack = Math.Min(invItem.maxStack, invItem.stack + item.stack);
				item.stack = newStack - invItem.stack;
				invItem.stack = newStack;

				if (item.stack <= 0)
				{
					item.TurnToAir();
					hasModified = true;
					break;
				}
			}
		}
		
		if (!hasModified)
		{
			return;
		}

		VirtualBagStoragePlayer storage = plr.GetModPlayer<VirtualBagStoragePlayer>();

		if (!storage.MatchedStorage.Remove(item))
		{
			storage.FilteredOutStorage.Remove(item);
		}

		RefreshStorage();
		SoundEngine.PlaySound(SoundID.Grab);
	}

	public static void UpdateSlot(Item item)
	{
		if (item.IsAir || item.type == ItemID.None)
		{
			return;
		}

		List<DrawableTooltipLine> lines = ItemTooltipBuilder.BuildTooltips(item, Main.LocalPlayer);
		float factor = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.5f + 0.5f;
		var color = Color.Lerp(Color.White, ItemTooltips.Colors.Positive, factor);
		var baseTip = new TooltipLine(PoTMod.Instance, "ClickNotice", Language.GetTextValue("Mods.PathOfTerraria.UI.VirtualBag.RightClick"));
		lines.Add(new DrawableTooltipLine(baseTip, lines.Count, 0, 0, color) { BaseScale = new(0.9f) });

		int nameLine = lines.FindIndex(x => x.Name == "ItemName");
		DrawableTooltipLine newLine = new(new TooltipLine(PoTMod.Instance, "ItemName", lines[nameLine].Text + $" (x{item.stack})"), 0, 0, 0, lines[nameLine].Color);
		lines[nameLine] = newLine;

		Tooltip.Create(new TooltipDescription
		{
			Identifier = "VirtualBagTooltip",
			AssociatedItem = item,
			Lines = lines,
		});
	}

	public void Toggle()
	{
		UIManager.TryToggleOrRegister(Identifier, "Vanilla: Mouse Text", new VirtualBagUIState(), 0, InterfaceScaleType.UI);
	}
}
