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

	public delegate ref Item[]? ItemInventoryGetter(Player player);
	
	/// <summary>
	///		The item that this slot wraps itself around.
	/// </summary>
	/// <remarks>
	///		Defaults to a new item with <see cref="ItemID.None"/> as its identity if
	///		<see cref="InventoryGetter"/> and <see cref="Slot"/> are not provided.
	/// </remarks>
	public Item? Item
	{
		get => InventoryGetter?.Invoke(Main.CurrentPlayer)[Slot] ?? item;
		set
		{
			if (InventoryGetter == null)
			{
				item = value;
			}
			else
			{
				InventoryGetter.Invoke(Main.CurrentPlayer)[Slot] = value;
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
	///		The index of the item that this slots wraps itself around.
	/// </summary>
	/// <remarks>
	///		Defaults to <c>0</c>. Will not have any effect if <see cref="InventoryGetter"/>
	///		is not provided.
	/// </remarks>
	public int Slot;
	
	/// <summary>
	///     The context of the item slot.
	/// </summary>
	/// <remarks>
	///     Defaults to <see cref="ItemSlot.Context.InventoryItem" />.
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
	///		Can be used to specify which item this slot should wrap itself around.
	/// </summary>
	public ItemInventoryGetter? InventoryGetter;
	
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

		OnInsertItem += (newItem, currentItem) => Icon.SetImage(newItem.IsAir ? iconTexture : TextureAssets.Item[newItem.type]);
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
	}

	private void UpdateInteraction()
	{
		var canInsert = Main.mouseItem.IsAir || Predicate?.Invoke(Main.mouseItem, Item) == true;
		
		if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface || !canInsert)
		{
			return;
		}

		if (InventoryGetter == null)
		{
			Item item = Item;
			
			ItemSlot.Handle(ref item, Context);
			
			Item = item;
		}
		else
		{
			ItemSlot.Handle(InventoryGetter.Invoke(Main.CurrentPlayer), Context, Slot);
		}
		
		Main.CurrentPlayer.mouseInterface = true;
	}
}