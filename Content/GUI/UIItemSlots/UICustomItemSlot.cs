using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.UIItemSlots;

public class UICustomItemSlot : UIElement
{
	// TODO: Document delegates, callbacks and predicates.
	public delegate bool ItemInsertionPredicate(Item newItem, Item currentItem);

	public delegate void ItemInsertionCallback(Item newItem, Item currentItem);

	public ItemInsertionPredicate? InsertionCallback;

	// TODO: Implement callbacks.
	public event ItemInsertionCallback? OnInsertItem;
	public event ItemInsertionCallback? OnRemoveItem;

	public virtual Item Item
	{
		get => item;
		set => item = value;
	}

	private Item item = new();

	private readonly Asset<Texture2D> backgroundTexture;
	private readonly Asset<Texture2D> iconTexture;

	/// <summary>
	///		The background of the item slot.
	/// </summary>
	public UIImage Background { get; private set; }
	
	/// <summary>
	///		The icon of the item slot.
	/// </summary>
	public UIImage Icon { get; private set; }
	
	/// <summary>
	///		The color used to render the background of the item slot.
	/// </summary>
	/// <remarks>
	///		Defaults to <see cref="Color.White"/>.
	/// </remarks>
	public readonly Color BackgroundColor;
	
	/// <summary>
	///		The context of the item slot.
	/// </summary>
	/// <remarks>
	///		Defaults to <see cref="ItemSlot.Context.InventoryItem"/>.
	/// </remarks>
	public readonly int Context;
	
	/// <summary>
	///		The scale of the item slot.
	/// </summary>
	/// <remarks>
	///		Defaults to <c>1f</c>.
	/// </remarks>
	public readonly float Scale;

	// TODO: Consider moving initialization logic to OnInitialize.
	public UICustomItemSlot(
		Asset<Texture2D> background,
		Asset<Texture2D> icon,
		Color? backgroundColor = null,
		int context = ItemSlot.Context.InventoryItem,
		float scale = 1f
	)
	{
		backgroundTexture = background;
		iconTexture = icon;
		
		BackgroundColor = backgroundColor ?? Color.White;
		Context = context;
		Scale = scale;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();
		
		Width.Set(backgroundTexture.Width() * Scale, 0f);
		Height.Set(backgroundTexture.Height() * Scale, 0f);
		
		Background = new UIImage(backgroundTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			Color = BackgroundColor,
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = StyleDimension.FromPixels(backgroundTexture.Width() * Scale),
			Height = StyleDimension.FromPixels(backgroundTexture.Height() * Scale),
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
		}
		else if (!Item.IsAir && Background.HasChild(Icon))
		{
			Background.RemoveChild(Icon);
		}
	}
	
	private void UpdateInteraction() 
	{
		if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface || InsertionCallback?.Invoke(Main.mouseItem, Item) == false)
		{
			return;
		}
		
		Main.CurrentPlayer.mouseInterface = true;

		Item item = Item;

		ItemSlot.Handle(ref item, Context);

		Item = item;
	}
}