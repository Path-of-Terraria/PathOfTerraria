using PathOfTerraria.Common.Looting.VirtualBagUI;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Content.Items.Currency.Runebound;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchUIState : UIState, IMutuallyExclusiveUI
{
	public const int SlotContext = ItemSlot.Context.ChestItem;
	public const string Identifier = "Currency Pouch UI";
	private const string CurrencyTab = "currency";
	private const string RunestonesTab = "runestones";
	private const int ItemsPerPage = 27;

	private static CurrencyPouchBackUI _backdrop = null;

	private static ref Item SlottedItem => ref Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().SlottedItem;

	private UIImageItemSlot _modifyItemSlot = null;
	private UIPanelTab _currencyTab = null;
	private UIPanelTab _runestonesTab = null;
	private string _activeTab = CurrencyTab;
	private int _page;

	public override void OnActivate()
	{
		const int GridBuffer = 50;
		const int TabBarHeight = 32;
		const string Path = "Mods.PathOfTerraria.UI.CurrencyPouch.";

		RemoveAllChildren();

		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<CurrencyPouchUIState>();

		UIPanel panel = new();
		panel.SetDimensions((0f, 0), (0, 0), (0, 540), (0, 320 + TabBarHeight));
		panel.VAlign = 0.5f;
		panel.HAlign = 0.5f;
		panel.OnUpdate += BlockClicks;
		panel.AddComponent(new UIMouseDrag(true, false));
		Append(panel);

		panel.Append(new UIText(Language.GetText(Path + "Title"), 0.6f, true) { Top = StyleDimension.FromPixels(6), Left = StyleDimension.FromPixels(6) });

		var close = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")) { HAlign = 1f };
		close.SetVisibility(1, 0.6f);
		close.SetDimensions((0, 0), (0, 0), (0, 38), (0, 38));
		close.OnLeftClick += Close;
		panel.Append(close);

		_currencyTab = new UIPanelTab(CurrencyTab, Language.GetText(Path + "TabCurrency"), 0.85f);
		_currencyTab.SetPadding(6);
		_currencyTab.Top = StyleDimension.FromPixels(GridBuffer);
		_currencyTab.BackgroundColor.A = 255;
		_currencyTab.OnLeftClick += (_, _) => SetActiveTab(CurrencyTab);
		panel.Append(_currencyTab);

		_runestonesTab = new UIPanelTab(RunestonesTab, Language.GetText(Path + "TabRunestones"), 0.85f);
		_runestonesTab.SetPadding(6);
		_runestonesTab.Top = StyleDimension.FromPixels(GridBuffer);
		_runestonesTab.BackgroundColor.A = 255;
		_runestonesTab.OnLeftClick += (_, _) => SetActiveTab(RunestonesTab);
		panel.Append(_runestonesTab);

		_currencyTab.Recalculate();
		_runestonesTab.Left = StyleDimension.FromPixels(_currencyTab.GetDimensions().Width + 8);
		_runestonesTab.Recalculate();
		UpdateTabStyles();

		_backdrop = new();
		_backdrop.SetDimensions((0, 0), (0, GridBuffer + TabBarHeight), (1, 0), (1, -(GridBuffer + TabBarHeight)));
		panel.Append(_backdrop);

		BuildItemSlots(_backdrop);

		UIElement container = _backdrop.AddElement(new UIElement(), x => x.SetDimensions((0.5f, -27), (0, 184), (0, 54), (0, 54)));

		var wrapper = new UIImageItemSlot.SlotWrapper(() => SlottedItem, x => SlottedItem = x);
		Asset<Texture2D> back = ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/CurrencyWeaponIcon", AssetRequestMode.ImmediateLoad);
		_modifyItemSlot = new UIImageItemSlot(back, TextureAssets.Npc[0], wrapper, SlotContext, null, true, 48);
		_modifyItemSlot.SetDimensions((0, 0), (0, 0), (1, 0), (1, 0));
		_modifyItemSlot.HAlign = 0.5f;
		_modifyItemSlot.VAlign = 0.5f;
		_modifyItemSlot.OnUpdate += DisplayConstantTooltipIfSlotted;
		container.Append(_modifyItemSlot);
	}

	private void DisplayConstantTooltipIfSlotted(UIElement affectedElement)
	{
		var slot = affectedElement as UIImageItemSlot;
		
		if (!slot.Item.IsAir)
		{
			List<DrawableTooltipLine> lines = ItemTooltipBuilder.BuildTooltips(slot.Item, Main.LocalPlayer);
			_backdrop.SetTooltips(lines);
		}
		else
		{
			_backdrop.SetTooltips(null);
		}
	}

	private void BuildItemSlots(CurrencyPouchBackUI backdrop)
	{
		CurrencyPouchStoragePlayer pouch = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>();
		IEnumerable<int> storedTypesQuery = pouch.StorageByType
			.Where(pair => pair.Value > 0 && BelongsToActiveTab(ContentSamples.ItemsByType[pair.Key]))
			.Select(pair => pair.Key);

		int[] storedTypes = (_activeTab == RunestonesTab
			? storedTypesQuery.OrderBy(type => type)
			: storedTypesQuery.OrderBy(type => ContentSamples.ItemsByType[type].Name))
			.ToArray();

		int pageCount = Math.Max(1, (storedTypes.Length + ItemsPerPage - 1) / ItemsPerPage);
		_page = Math.Clamp(_page, 0, pageCount - 1);
		int start = _page * ItemsPerPage;
		int end = Math.Min(start + ItemsPerPage, storedTypes.Length);

		for (int i = start; i < end; i++)
		{
			int slot = i - start;
			Vector2 position = new(14 + slot % 9 * 57, 8 + slot / 9 * 52);
			backdrop.Append(BuildSingleSlot(storedTypes[i], position));
		}

		if (pageCount > 1)
		{
			var previous = new UITextPanel<string>("<", 0.8f) { Left = StyleDimension.FromPixels(188), Top = StyleDimension.FromPixels(200) };
			previous.SetDimensions((0, 0), (0, 0), (0, 34), (0, 34));
			previous.OnLeftClick += (_, _) => ChangePage(-1);
			backdrop.Append(previous);

			var pageText = new UIText($"{_page + 1} / {pageCount}", 0.75f) { HAlign = 0.5f, Top = StyleDimension.FromPixels(160) };
			backdrop.Append(pageText);

			var next = new UITextPanel<string>(">", 0.8f) { Left = StyleDimension.FromPixels(318), Top = StyleDimension.FromPixels(200) };
			next.SetDimensions((0, 0), (0, 0), (0, 34), (0, 34));
			next.OnLeftClick += (_, _) => ChangePage(1);
			backdrop.Append(next);
		}
	}

	private bool BelongsToActiveTab(Item item)
	{
		return item.ModItem is CurrencyShard
			&& (_activeTab == RunestonesTab ? item.ModItem is Runestone : item.ModItem is not Runestone);
	}

	private void SetActiveTab(string tab)
	{
		if (_activeTab == tab)
		{
			return;
		}

		_activeTab = tab;
		_page = 0;
		OnActivate();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void UpdateTabStyles()
	{
		_currencyTab.TextColor = _activeTab == CurrencyTab ? Color.Yellow : Color.White;
		_runestonesTab.TextColor = _activeTab == RunestonesTab ? Color.Yellow : Color.White;
	}

	private void ChangePage(int direction)
	{
		_page += direction;
		OnActivate();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	public UIItemIcon BuildSingleSlot(int type, Vector2 position)
	{
		position -= new Vector2(1, 4);

		Item item = ContentSamples.ItemsByType[type];

		var storageIcon = new UIItemIcon(item, false)
		{
			Width = StyleDimension.FromPixels(42),
			Height = StyleDimension.FromPixels(42),
			Left = StyleDimension.FromPixels(position.X),
			Top = StyleDimension.FromPixels(position.Y),
		};

		storageIcon.OnUpdate += (self) =>
		{
			if (self.ContainsPoint(Main.MouseScreen))
			{
				UpdateSlot(item);
			}
		};

		UIText stack = new("x1", 0.75f) { Left = StyleDimension.FromPixels(30), Top = StyleDimension.FromPixels(30) };
		storageIcon.Append(stack);
		stack.AddComponent(new UIDynamicText(x =>
		{
			int amount = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType[type];
			(x as UIText).TextColor = amount == Item.CommonMaxStack ? Color.Red : Color.White;
			return "x" + amount.ToString();
		}));

		storageIcon.OnLeftClick += (_, _) => LeftClickItem(item);
		storageIcon.OnRightClick += (_, _) => RightClickItem(item);

		return storageIcon;
	}

	private void LeftClickItem(Item item)
	{
		bool isControlHeld = Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl);
		bool isShiftHeld = Main.keyState.PressingShift();

		if (isShiftHeld && !isControlHeld)
		{
			if (!TryStoreAllMatchingInventoryItems(item.type, out int stored))
			{
				return;
			}

			SoundEngine.PlaySound(SoundID.Grab);
			return;
		}

		if (!isControlHeld)
		{
			return;
		}

		int amountToExtract = isShiftHeld ? Item.CommonMaxStack : 1;

		if (!TryExtractItem(item, amountToExtract, out int extracted))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.Grab);

		if (extracted > 0 && !Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType.ContainsKey(item.type))
		{
			Toggle();
			Toggle();
		}
	}

	private void RightClickItem(Item item)
	{
		if (item.ModItem is not CurrencyShard shard)
		{
			return;
		}

		if (!shard.CanUseInPouch(SlottedItem, out _))
		{
			return;
		}

		Dictionary<int, int> storage = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;
		
		// Check if player actually has any of this currency type
		if (!storage.TryGetValue(item.type, out int currentAmount) || currentAmount <= 0)
		{
			return;
		}

		storage[item.type] = Math.Max(storage[item.type] - 1, 0);
		shard.ApplyToItem(SlottedItem);

		if (storage[item.type] == 0)
		{
			Toggle();
			Toggle();
		}
	}

	private static bool TryExtractItem(Item item, int requestedAmount, out int extracted)
	{
		extracted = 0;

		Dictionary<int, int> storage = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;

		if (!storage.TryGetValue(item.type, out int currentAmount) || currentAmount <= 0)
		{
			return false;
		}

		int remainingToInsert = Math.Min(requestedAmount, currentAmount);
		Player player = Main.LocalPlayer;

		for (int i = 0; i < Main.InventoryItemSlotsCount && remainingToInsert > 0; i++)
		{
			ref Item invItem = ref player.inventory[i];

			if (invItem.type != item.type || invItem.stack >= invItem.maxStack)
			{
				continue;
			}

			int moved = Math.Min(invItem.maxStack - invItem.stack, remainingToInsert);
			invItem.stack += moved;
			remainingToInsert -= moved;
			extracted += moved;
		}

		for (int i = 0; i < Main.InventoryItemSlotsCount && remainingToInsert > 0; i++)
		{
			ref Item invItem = ref player.inventory[i];

			if (!invItem.IsAir)
			{
				continue;
			}

			invItem = item.Clone();
			invItem.stack = Math.Min(item.maxStack, remainingToInsert);
			remainingToInsert -= invItem.stack;
			extracted += invItem.stack;
		}

		if (extracted <= 0)
		{
			return false;
		}

		storage[item.type] -= extracted;

		if (storage[item.type] <= 0)
		{
			storage.Remove(item.type);
		}

		return true;
	}

	private static bool TryStoreAllMatchingInventoryItems(int itemType, out int stored)
	{
		stored = 0;

		Player player = Main.LocalPlayer;
		Dictionary<int, int> storage = player.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;
		storage.TryAdd(itemType, 0);

		for (int i = 0; i < Main.InventoryItemSlotsCount; i++)
		{
			ref Item invItem = ref player.inventory[i];

			if (invItem.type != itemType || invItem.IsAir)
			{
				continue;
			}

			int storageSpace = invItem.maxStack - storage[itemType];

			if (storageSpace <= 0)
			{
				break;
			}

			int moved = Math.Min(invItem.stack, storageSpace);
			storage[itemType] += moved;
			invItem.stack -= moved;
			stored += moved;

			if (invItem.stack <= 0)
			{
				invItem.TurnToAir();
			}
		}

		if (storage[itemType] <= 0)
		{
			storage.Remove(itemType);
		}

		return stored > 0;
	}

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
	}

	public static void UpdateSlot(Item item)
	{
		if (item.IsAir || item.type == ItemID.None)
		{
			return;
		}

		List<DrawableTooltipLine> lines = ItemTooltipBuilder.BuildTooltips(item, Main.LocalPlayer);

		if (!SlottedItem.IsAir)
		{
			var shard = item.ModItem as CurrencyShard;
			string text = Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.Apply");
			Color fadeColor = ItemTooltips.Colors.Positive;

			if (!shard.CanUseInPouch(SlottedItem, out string failKey))
			{
				fadeColor = ItemTooltips.Colors.Negative;
				text = Language.GetTextValue("Mods.PathOfTerraria.Misc.ShardInvalidations." + failKey);
			}

			float factor = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.5f + 0.5f;
			var color = Color.Lerp(Color.White, fadeColor, factor);
			var baseTip = new TooltipLine(PoTMod.Instance, "ApplyNotice", text);
			int stack = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType[item.type];
			lines.Add(new DrawableTooltipLine(baseTip, lines.Count, 0, 0, color) { BaseScale = new(0.9f) });
		}

		lines.Add(new DrawableTooltipLine(
			new TooltipLine(PoTMod.Instance, "ExtractOne", Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.ExtractOne")),
			lines.Count, 0, 0, ItemTooltips.Colors.Positive) { BaseScale = new(0.9f) });
		lines.Add(new DrawableTooltipLine(
			new TooltipLine(PoTMod.Instance, "ExtractAll", Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.ExtractAll")),
			lines.Count, 0, 0, ItemTooltips.Colors.Positive) { BaseScale = new(0.9f) });
		lines.Add(new DrawableTooltipLine(
			new TooltipLine(PoTMod.Instance, "StoreAll", Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.StoreAll")),
			lines.Count, 0, 0, ItemTooltips.Colors.Positive) { BaseScale = new(0.9f) });

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

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		float oldScale = Main.inventoryScale;
		Main.inventoryScale = 1f;
		base.DrawChildren(spriteBatch);
		Main.inventoryScale = oldScale;
	}
}
