using PathOfTerraria.Common.Looting.VirtualBagUI;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.Armor.Elements;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchUIState : UIState, IMutuallyExclusiveUI
{
	public const int SlotContext = ItemSlot.Context.ChestItem;
	public const string Identifier = "Currency Pouch UI";

	//private static readonly Item[] _backingSlot = [new()];

	private static CurrencyPouchBackUI _backdrop = null;

	private static ref Item SlottedItem => ref Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().SlottedItem;

	private UIImageItemSlot _modifyItemSlot = null;

	public override void OnActivate()
	{
		const int GridBuffer = 50;
		const string Path = "Mods.PathOfTerraria.UI.CurrencyPouch.";

		RemoveAllChildren();

		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<CurrencyPouchUIState>();

		UIPanel panel = new();
		panel.SetDimensions((0f, 0), (0, 0), (0, 424), (0, 212));
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

		_backdrop = new();
		_backdrop.SetDimensions((0, 0), (0, GridBuffer), (1, 0), (1, -GridBuffer));
		panel.Append(_backdrop);

		BuildItemSlots(_backdrop);

		UIElement container = _backdrop.AddElement(new UIElement(), x => x.SetDimensions((0, 170), (0, 62), (0, 54), (0, 54)));

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
		TryAppendSingleItem<UnfoldingShard>(backdrop, new Vector2(20, 12));
		TryAppendSingleItem<GlimmeringShard>(backdrop, new Vector2(70, 12));
		TryAppendSingleItem<MysticShard>(backdrop, new Vector2(124, 12));
		TryAppendSingleItem<LimpidShard>(backdrop, new Vector2(120, 72));
		TryAppendSingleItem<CorruptShard>(backdrop, new Vector2(180, 11));
		TryAppendSingleItem<AscendantShard>(backdrop, new Vector2(238, 12));
		TryAppendSingleItem<ShiftingShard>(backdrop, new Vector2(242, 76));
		TryAppendSingleItem<RadiantShard>(backdrop, new Vector2(290, 12));
		TryAppendSingleItem<EchoingShard>(backdrop, new Vector2(342, 14));
	}

	private void TryAppendSingleItem<T>(CurrencyPouchBackUI backdrop, Vector2 position) where T : CurrencyShard
	{
		UIItemIcon icon = BuildSingleSlot<T>(position);

		if (icon is not null)
		{
			backdrop.Append(icon);
		}
	}

	public UIItemIcon BuildSingleSlot<T>(Vector2 position) where T : CurrencyShard
	{
		int type = ModContent.ItemType<T>();
		position -= new Vector2(1, 4);

		if (!Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType.TryGetValue(type, out int count))
		{
			return null;
		}

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

		storageIcon.OnRightClick += (_, _) => RightClickItem(item);

		return storageIcon;
	}

	private void RightClickItem(Item item)
	{
		var shard = item.ModItem as CurrencyShard;

		if (!shard.CanUseInPouch(SlottedItem, out _))
		{
			return;
		}

		Dictionary<int, int> storage = Main.LocalPlayer.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;
		storage[item.type] = Math.Max(storage[item.type] - 1, 0);
		shard.ApplyToItem(SlottedItem);

		if (storage[item.type] == 0)
		{
			Toggle();
			Toggle();
		}
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
