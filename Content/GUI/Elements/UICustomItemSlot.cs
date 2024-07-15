using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Elements;

/// <summary>
///		Provides an item slot wrapper as a <see cref="UIElement"/>.
/// </summary>
/// <remarks>
///		This wrapper allows you to have an independant item for the slot, or to
///		wrap around an existing array of items at a given index through
///		<see cref="InventoryGetter"/> and <see cref="Slot"/>.
/// </remarks>
public class UICustomItemSlot : UIElement
{
	public delegate void ItemInsertionCallback(Item newItem, Item currentItem);

	public delegate bool ItemInsertionPredicate(Item newItem, Item currentItem);

	/// <summary>
	///		The item that this slot wraps itself around.
	/// </summary>
	/// <remarks>
	///		Defaults to a new item with <see cref="ItemID.None"/> as its identity if
	///		<see cref="InventoryGetter"/> and <see cref="Slot"/> are not provided.
	/// </remarks>
	public Item? Item
	{
		get => WrapsAroundInventory ? Inventory[Slot] : item;
		set
		{
			if (WrapsAroundInventory)
			{
				OnInsertItem?.Invoke(value, Inventory[Slot]);
				
				Inventory[Slot] = value;
			}
			else
			{
				OnInsertItem?.Invoke(value, item);
				
				item = value;
			}
		}
	}

	/// <summary>
	///		Whether the item slot wraps itself around an inventory or not.
	/// </summary>
	public bool WrapsAroundInventory => Inventory != null && Slot > -1;

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
	///		The index of the item that the slots wraps itself around.
	/// </summary>
	/// <remarks>
	///		Will not have any effect if <see cref="Inventory"/> is <c>null</c> or not provided.
	/// </remarks>
	public readonly int Slot;
	
	/// <summary>
	///		The inventory that the slots wraps itself around.
	/// </summary>
	public readonly Item[]? Inventory;
	
	/// <summary>
	///     The context of the item slot.
	/// </summary>
	/// <remarks>
	///     Defaults to <see cref="ItemSlot.Context.InventoryItem"/>.
	/// </remarks>
	public readonly int Context;
	
	private Item item = new();
	
	private readonly Asset<Texture2D> backgroundTexture;
	private readonly Asset<Texture2D> iconTexture;

	/// <summary>
	///     Can be used to determine whether an item can be inserted into the slot or not.
	/// </summary>
	public ItemInsertionPredicate? Predicate;

	/// <summary>
	///     Can be used to register a callback to execute logic when an item is inserted into the slot.
	/// </summary>
	public event ItemInsertionCallback? OnInsertItem;
	
	public UICustomItemSlot(
		Asset<Texture2D> backgroundTexture,
		Asset<Texture2D> iconTexture,
		int context = ItemSlot.Context.InventoryItem
	)
	{
		this.backgroundTexture = backgroundTexture;
		this.iconTexture = iconTexture;

		Context = context;
	}
	
	public UICustomItemSlot(
		Asset<Texture2D> backgroundTexture,
		Asset<Texture2D> iconTexture,
		ref Item[]? inventory,
		int slot,
		int context = ItemSlot.Context.InventoryItem
	) : this(backgroundTexture, iconTexture, context)
	{
		Inventory = inventory;
		Slot = slot;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width.Set(backgroundTexture.Width() * Scale, 0f);
		Height.Set(backgroundTexture.Height() * Scale, 0f);

		Background = new UIImage(backgroundTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			NormalizedOrigin = new Vector2(0.5f),
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = StyleDimension.FromPixels(backgroundTexture.Width() * Scale),
			Height = StyleDimension.FromPixels(backgroundTexture.Height() * Scale)
		};

		Append(Background);

		Icon = new UIImage(iconTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			NormalizedOrigin = new Vector2(0.5f),
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
		
		Icon.SetImage(Item.IsAir ? iconTexture : TextureAssets.Item[Item.type]);
	}

	private void UpdateInteraction()
	{
		if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface || (!Main.mouseItem.IsAir && Predicate?.Invoke(Main.mouseItem, Item) == false))
		{
			return;
		}

		if (WrapsAroundInventory)
		{
			ItemSlot.Handle(Inventory, Context, Slot);
		}
		else
		{
			Item item = Item;
			
			ItemSlot.Handle(ref item, Context);
			
			Item = item;
		}
		
		Main.CurrentPlayer.mouseInterface = true;
	}
}