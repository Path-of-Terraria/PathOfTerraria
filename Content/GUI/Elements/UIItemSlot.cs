using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Elements;

/// <summary>
///		Provides an item slot wrapper as a <see cref="UIElement"/>.
/// </summary>
public class UIItemSlot : UIElement
{
	public delegate void ItemInsertionCallback(Item newItem, Item currentItem);

	public delegate bool ItemInsertionPredicate(Item newItem, Item currentItem);

	public delegate ref Item? ItemGetterCallback(Player player);
	
	/// <summary>
	///		The item that this slot wraps itself around.
	/// </summary>
	/// <remarks>
	///		Defaults to a new item if <see cref="ItemGetter"/> is not specified.
	/// </remarks>
	public Item? Item
	{
		get => ItemGetter?.Invoke(Main.CurrentPlayer) ?? item;
		set
		{
			if (ItemGetter == null)
			{
				item = value;
			}
			else
			{
				ItemGetter.Invoke(Main.CurrentPlayer) = value;
			}
		}
	}

	/// <summary>
	///     The background of the item slot.
	/// </summary>
	public UIImage Background { get; private set; }

	/// <summary>
	///     The icon of the item slot.
	/// </summary>
	public UIImage Icon { get; private set; }

	/// <summary>
	///     The scale of the item slot.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>1f</c>.
	/// </remarks>
	public float Scale = 1f;

	/// <summary>
	///     The context of the item slot.
	/// </summary>
	/// <remarks>
	///     Defaults to <see cref="ItemSlot.Context.InventoryItem" />.
	/// </remarks>
	public readonly int Context;
	
	private readonly Asset<Texture2D> backgroundTexture;
	private readonly Asset<Texture2D> iconTexture;

	private bool iconActive = true;

	/// <summary>
	///     Can be used to determine whether an item can be inserted into the slot or not.
	/// </summary>
	public ItemInsertionPredicate? Predicate;
	
	/// <summary>
	///		Can be used to specify which item this slot should wrap itself around.
	/// </summary>
	public ItemGetterCallback? ItemGetter;
	
	/// <summary>
	///     Can be used to register a callback to execute logic when an item is inserted into the slot.
	/// </summary>
	public event ItemInsertionCallback? OnInsertItem;

	protected Item item = new();

	public UIItemSlot(
		Asset<Texture2D> backgroundTexture,
		Asset<Texture2D> iconTexture,
		int context = ItemSlot.Context.InventoryItem
	)
	{
		this.backgroundTexture = backgroundTexture;
		this.iconTexture = iconTexture;

		Context = context;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width.Set(backgroundTexture.Width() * Scale, 0f);
		Height.Set(backgroundTexture.Height() * Scale, 0f);

		Background = new UIImage(backgroundTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = StyleDimension.FromPixels(backgroundTexture.Width() * Scale),
			Height = StyleDimension.FromPixels(backgroundTexture.Height() * Scale)
		};

		Append(Background);

		Icon = new UIImage(iconTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = StyleDimension.FromPixels(iconTexture.Width() * Scale),
			Height = StyleDimension.FromPixels(iconTexture.Height() * Scale)
		};

		Background.Append(Icon);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Background.ImageScale = Scale;
		Icon.ImageScale = Scale;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		UpdateInteraction();
		UpdateSlotIcon();

		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, default, default);

		ItemSlot.DrawItemIcon(Item, Context, spriteBatch, GetDimensions().Center(), Scale * Item.scale, 32f, Color.White);

		spriteBatch.End();
		spriteBatch.Begin();
	}

	private void UpdateSlotIcon()
	{
		if (Item.IsAir && !Background.HasChild(Icon))
		{
			Background.Append(Icon);

			Recalculate();

			iconActive = true;
		}
		else if (!Item.IsAir && Background.HasChild(Icon))
		{
			iconActive = false;
		}

		Icon.Color = Color.Lerp(Icon.Color, iconActive ? Color.White : Color.Transparent, 0.2f);

		if (Icon.Color != Color.Transparent)
		{
			return;
		}

		Background.RemoveChild(Icon);

		Recalculate();
	}

	private void UpdateInteraction()
	{
		var canInsert = Main.mouseItem.IsAir || Predicate?.Invoke(Main.mouseItem, Item) == true;
		
		if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface || !canInsert)
		{
			return;
		}

		Main.CurrentPlayer.mouseInterface = true;

		Item item = Item;

		ItemSlot.Handle(ref item, ItemSlot.Context.ChestItem);

		Item = item;
	}
}