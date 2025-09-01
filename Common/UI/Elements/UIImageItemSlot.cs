using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///     Provides an item slot wrapper as a <see cref="UIElement" />.
/// </summary>
public class UIImageItemSlot : UIElement
{
	public delegate void ItemInsertionCallback(Item newItem, Item currentItem);

	public delegate bool ItemInsertionPredicate(Item newItem, Item currentItem);

	public readonly struct SlotWrapper
	{
		public readonly Func<Item> Get;
		public readonly Action<Item> Set;
		public readonly Func<(Item[] Inventory, int Slot)> ByInventory;

		public SlotWrapper(Func<Item> get, Action<Item> set)
		{
			(Get, Set) = (get, set);
		}
		public SlotWrapper(Func<(Item[] Inventory, int Slot)> byInventory)
		{
			ByInventory = byInventory;
			Get = () => byInventory().Inventory[byInventory().Slot];
			Set = value => byInventory().Inventory[byInventory().Slot] = value;
		}
	}

	private readonly SlotWrapper handler;

	/// <summary>
	///     The item that this slot wraps itself around.
	/// </summary>
	public Item Item
	{
		get => handler.Get();
		set => handler.Set(value);
	}

	/// <summary>
	///     The background of the item slot.
	/// </summary>
	public UIImage Background { get; protected set; }

	/// <summary>
	///     The icon of the item slot.
	/// </summary>
	public UIImage Icon { get; protected set; }

	protected Asset<Texture2D> BackgroundTexture;

	/// <summary>
	///     The context of the item slot.
	/// </summary>
	/// <remarks>
	///     Defaults to <see cref="ItemSlot.Context.InventoryItem" />.
	/// </remarks>
	public int Context;

	protected Asset<Texture2D> IconTexture;

	/// <summary>
	///     The inventory that the slots wraps itself around.
	/// </summary>
	public Item[] Inventory;

	/// <summary>
	///     Can be used to determine whether an item can be inserted into the slot or not.
	/// </summary>
	public ItemInsertionPredicate? Predicate;

	/// <summary>
	///    The key to look for in Localization for tooltip hover
	/// </summary>
	/// <remarks>
	///     Will not have any effect if <see cref="Key" /> is <c>null</c> or not provided.
	/// </remarks>
	public string Key;

	public UIImageItemSlot(
		Asset<Texture2D> backgroundTexture,
		Asset<Texture2D> iconTexture,
		SlotWrapper itemHandler,
		int context = ItemSlot.Context.InventoryItem,
		string key = null
	)
	{
		BackgroundTexture = backgroundTexture;
		IconTexture = iconTexture;
		handler = itemHandler;
		Context = context;
		Key = key;
	}

	/// <summary>
	///     Can be used to register a callback to execute logic when an item is inserted into the slot.
	/// </summary>
	public event ItemInsertionCallback? OnInsertItem;

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width.Set(BackgroundTexture.Width(), 0f);
		Height.Set(BackgroundTexture.Height(), 0f);

		Background = new UIImage(BackgroundTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		Append(Background);

		Icon = new UIImage(IconTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		Background.Append(Icon);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		// FIX: This offsets the icon being drawn to the left in order to center it
		// relative to the background frame.  By default, it's positioned in such a
		// way that the leftmost portion of the item is aligned with the leftmost
		// portion of the frame, causing the right side of the item to protrude out
		// and offsetting it.  This offset messes up the traditional logic for
		// scaling and centering the item, so we have to manually offset the item
		// left to counteract this automatic positioning.
		Asset<Texture2D> tex = GetIconToDraw();
		if (tex.Width() > BackgroundTexture.Width())
		{
			Icon.Left = StyleDimension.FromPixels(-(tex.Width() - BackgroundTexture.Width()) / 2f);
		}
		else
		{
			Icon.Left = StyleDimension.FromPixels(0);
		}

		if (tex.Height() > BackgroundTexture.Height())
		{
			Icon.Top = StyleDimension.FromPixels(-(tex.Height() - BackgroundTexture.Height()) / 2f);
		}
		else
		{
			Icon.Top = StyleDimension.FromPercent(0);
		}
		
		UpdateIcon();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		UpdateInteraction();
	}

	protected virtual void UpdateIcon()
	{
		if (!Item.IsAir)
		{
			Texture2D texture = TextureAssets.Item[Item.type].Value;
			Rectangle frame = Main.itemAnimations[Item.type] == null ? texture.Frame() : Main.itemAnimations[Item.type].GetFrame(texture);

			ItemSlot.DrawItem_GetColorAndScale(Item, Item.scale, ref Icon.Color, 32f, ref frame, out _, out float finalDrawScale);

			Icon.ImageScale = finalDrawScale;
		}
		else
		{
			Icon.ImageScale = 1f;
		}

		Icon.SetImage(GetIconToDraw());
	}

	protected virtual void UpdateInteraction()
	{
		if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface || !Main.mouseItem.IsAir && Predicate?.Invoke(Main.mouseItem, Item) == false)
		{
			return;
		}

		if (handler.ByInventory?.Invoke() is { } inv)
		{
			ItemSlot.Handle(inv.Inventory, slot: inv.Slot, context: Context);
		}
		else
		{
			Item item = Item;
			ItemSlot.Handle(ref item, context: Context);
			Item = item;
		}

		if (Key != null)
		{
			Main.hoverItemName = Language.GetTextValue(Key);
			Main.HoverItem = Item.Clone();
			Main.HoverItem.tooltipContext = Context;
		}

		Main.LocalPlayer.mouseInterface = true;
	}

	// Because we can't access the texture being used by the UIImage.
	protected virtual Asset<Texture2D> GetIconToDraw()
	{
		if (Item.IsAir)
		{
			return IconTexture;
		}
		
		Main.instance.LoadItem(Item.type);
		
		return TextureAssets.Item[Item.type];
	}
}