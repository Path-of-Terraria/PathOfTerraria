using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Utilities;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.Elements;

#nullable enable

/// <summary>
///     Provides an item slot wrapper as a <see cref="UIElement" />.
/// </summary>
public class UIImageItemSlot
(
	Asset<Texture2D> backgroundTexture,
	Asset<Texture2D> iconTexture,
	UIImageItemSlot.SlotWrapper itemHandler,
	int context = ItemSlot.Context.InventoryItem,
	(string Key, object Arg0)? hoverText = null,
	bool skipAutoSizing = false,
	float iconScalingSize = UIImageItemSlot.DefaultIconSize
) : UIElement
{
	public const float DefaultIconSize = 24f;

	public delegate void ItemUpdateCallback(UIElement element, Item oldItem, Item newItem);

	public delegate bool IsLockedPredicate(UIImageItemSlot slot);

	public delegate bool ItemInsertionPredicate(Item newItem, Item currentItem);

	public readonly struct SlotWrapper
	{
		public readonly Func<Item> Get;
		public readonly Action<Item> Set;
		public readonly Func<(Item[] Inventory, int Slot)>? ByInventory;

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

	private static Asset<Texture2D>? FavoriteBack = null;

	protected readonly float IconSize = iconScalingSize;

	private readonly SlotWrapper handler = itemHandler;
	private readonly bool skipAutoSize = skipAutoSizing;

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
	public UIImage Background { get; protected set; } = null!;

	/// <summary>
	///     The icon of the item slot.
	/// </summary>
	public UIHoverImage Icon { get; protected set; } = null!;

	public Asset<Texture2D> BackgroundTexture { get; set; } = backgroundTexture;

	/// <summary>
	///     The context of the item slot.
	/// </summary>
	/// <remarks>
	///     Defaults to <see cref="ItemSlot.Context.InventoryItem" />.
	/// </remarks>
	public int Context = context;

	public Asset<Texture2D> IconTexture { get; set; } = iconTexture;

	/// <summary>
	///     Can be used to determine whether an item can be inserted into the slot or not.
	/// </summary>
	public ItemInsertionPredicate? Predicate { get; set; }

	/// <summary>
	/// Can be used to determine whether an item can be inserted into the slot or not.
	/// </summary>
	public IsLockedPredicate? IsLocked { get; set; }

	/// <summary> Controls whether to render the stack value. </summary>
	public bool DrawStack { get; set; } = true;

	/// <summary>
	///    The localization key and optional argument to use for tooltip hover.
	/// </summary>
	/// <remarks>
	///     Will not have any effect if <see cref="HoverText" /> is <c>null</c> or not provided.
	/// </remarks>
	public (string Key, object Arg0)? HoverText = hoverText;

	/// <summary>
	///     Can be used to register a callback to execute logic when an item is inserted into the slot.
	/// </summary>
	public event ItemUpdateCallback? OnModifyItem;

	public override void OnInitialize()
	{
		base.OnInitialize();

		if (!skipAutoSize)
		{
			Width.Set(BackgroundTexture.Width(), 0f);
			Height.Set(BackgroundTexture.Height(), 0f);
		}

		Background = new UIImage(BackgroundTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		Append(Background);

		Icon = new(IconTexture)
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
		CalculatedStyle dims = GetDimensions();

		if (tex.Width() > BackgroundTexture.Width() && !skipAutoSize)
		{
			Icon.Left = StyleDimension.FromPixels(-(tex.Width() - BackgroundTexture.Width()) / 2f);
		}
		else
		{
			Icon.Left = StyleDimension.FromPixels(0);
		}

		if (tex.Height() > BackgroundTexture.Height() && !skipAutoSize)
		{
			Icon.Top = StyleDimension.FromPixels(-(tex.Height() - BackgroundTexture.Height()) / 2f);
		}
		else
		{
			Icon.Top = StyleDimension.FromPixels(0);
		}

		UpdateIcon();
	}

	public override void Draw(SpriteBatch sb)
	{
		UpdateInteraction();

		base.Draw(sb);

		if (Item is { IsAir: false })
		{
			DrawItem(sb);
		}
	}

	protected virtual void DrawItem(SpriteBatch sb)
	{
		Main.instance.LoadItem(Item.type);

		CalculatedStyle dimensions = GetDimensions();
		Vector2 uiSize = new(dimensions.Width, dimensions.Height);
		Vector2 center = dimensions.Center();

		if (Icon.RemoveFloatingPointsFromDrawPosition)
		{
			center = center.Floor();
		}

		float baseScale = 1f * Icon.ImageScale;
		float sizeLimit = IconSize; //Math.Min(dimensions.Width, dimensions.Height);

		if (Item.favorited)
		{
			FavoriteBack ??= ModContent.Request<Texture2D>("PathOfTerraria/Assets/Slots/FavoriteOverlay");
			Texture2D tex = FavoriteBack.Value;
			Main.spriteBatch.Draw(tex, center, null, Color.White, 0f, tex.Size() / 2f, MathHelper.Lerp(baseScale, 0.8f, 0.5f), SpriteEffects.None, 0);
		}

		ItemSlot.DrawItemIcon(Item, ItemSlot.Context.InventoryItem, sb, center, baseScale, sizeLimit, Color.White);

		if (DrawStack && Item.stack > 1)
		{
			float invScale = 1f;
			Vector2 stackPos = (dimensions.Position() + new Vector2(dimensions.Width, dimensions.Height) * new Vector2(0.1f, 0.55f)) * invScale;
			ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.ItemStack.Value, Item.stack.ToString(), stackPos, Color.White, 0f, Vector2.Zero, new Vector2(invScale), -1f, invScale);
		}
	}

	protected virtual void UpdateIcon()
	{
		Icon.SetImage(GetIconToDraw());
		Background.SetImage(BackgroundTexture);
	}

	protected virtual void UpdateInteraction()
	{
		if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface)
		{
			return;
		}

		if (IsLocked?.Invoke(this) == true)
		{
			return;
		}

		if (!Main.mouseItem.IsAir && Predicate?.Invoke(Main.mouseItem, Item) == false)
		{
			return;
		}

		Item item = Item;
		(Item oldItem, int oldType, int oldStack, int oldPrefix) = (item, item.type, item.stack, item.prefix);

		using var _ = ValueOverride.Create(ref BlockChestItemSyncing.Blocking, true);

		if (handler.ByInventory?.Invoke() is { } inv)
		{
			ItemSlot.Handle(inv.Inventory, slot: inv.Slot, context: Context);
			item = Item;
		}
		else
		{
			ItemSlot.Handle(ref item, context: Context);
			Item = item;
		}

		// Invoke event if the item is perceived as modified.
		if (item != oldItem || item.type != oldType || item.stack != oldStack || item.prefix != oldPrefix)
		{
			OnModifyItem?.Invoke(this, oldItem, item);
		}

		if (HoverText is { } hoverText)
		{
			Main.hoverItemName = hoverText.Arg0 != null ? Language.GetTextValue(hoverText.Key, hoverText.Arg0) : Language.GetTextValue(hoverText.Key);
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

		return Asset<Texture2D>.Empty;
	}
}