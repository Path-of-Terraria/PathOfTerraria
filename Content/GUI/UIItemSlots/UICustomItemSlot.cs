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

	/// <summary>
	///		The background of the item slot.
	/// </summary>
	public readonly UIImage Background;
	
	/// <summary>
	///		The icon of the item slot.
	/// </summary>
	public readonly UIImage Icon;
	
	/// <summary>
	///		The color used to render the background of the item slot.
	/// </summary>
	/// <remarks>
	///		Defaults to <see cref="Color.White"/>.
	/// </remarks>
	public readonly Color BackgroundColor;
	
	/// <summary>
	///		The context for the item slot.
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
	public UICustomItemSlot(Asset<Texture2D> background, Asset<Texture2D> icon, Color? backgroundColor = null, int context = ItemSlot.Context.InventoryItem, float scale = 1f)
	{
		BackgroundColor = backgroundColor ?? Color.White;	
		Context = context;
		Scale = scale;
		
		Width.Set(background.Width() * scale, 0f);
		Height.Set(background.Height() * scale, 0f);
		
		Background = new UIImage(background)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			Color = BackgroundColor,
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = StyleDimension.FromPixels(background.Width() * scale),
			Height = StyleDimension.FromPixels(background.Height() * scale),
		};
		
		Append(Background);

		Icon = new UIImage(icon)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = StyleDimension.FromPixels(icon.Width() * scale),
			Height = StyleDimension.FromPixels(icon.Height() * scale)
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