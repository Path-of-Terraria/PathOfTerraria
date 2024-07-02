using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.UIItemSlots;

/// <summary>
/// Used to allow items to be inserted. If null, defaults to true
/// </summary>
internal delegate bool CanItemBeInsertedDelegate(Item newItem, Item currentItem);

internal class UICustomItemSlot : UIElement
{
	internal CanItemBeInsertedDelegate CanItemBeInserted = null;

	internal virtual Item Item
	{
		get => _item;
		set => _item = value;
	}

	private Item _item = new();
	private readonly Asset<Texture2D> _slotBackground;
	private readonly Color _slotBackgroundColor;
	private readonly Asset<Texture2D> _slotIcon;
	private readonly int _itemSlotContext;
	private readonly float _scale;

	public UICustomItemSlot(Asset<Texture2D> slotBackground, Asset<Texture2D> slotIcon, Color? slotBackgroundColor = null, int itemSlotContext = ItemSlot.Context.InventoryItem, float scale = 1f)
	{
		_slotBackground = slotBackground;
		_slotBackgroundColor = slotBackgroundColor ?? Color.White;
		_slotIcon = slotIcon;
		_itemSlotContext = itemSlotContext;
		_scale = scale;
		
		Width.Set(slotBackground.Width() * scale, 0f);
		Height.Set(slotBackground.Width() * scale, 0f);
	}

	internal void ForceSetItem(Item newItem)
	{
		Item = newItem ?? new Item();
	}

	private void HandleInteraction()
	{
		Main.CurrentPlayer.mouseInterface = true;

		Item item = Item;
		ItemSlot.Handle(ref item, _itemSlotContext);
		Item = item;
	}
	
	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		
		if (IsMouseHovering && !PlayerInput.IgnoreMouseInterface && CanItemBeInserted?.Invoke(Main.mouseItem, Item) != false)
		{
		    HandleInteraction();
		}

		Vector2 position = GetDimensions().ToRectangle().TopLeft();
		spriteBatch.Draw(_slotBackground.Value, position, null, _slotBackgroundColor, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0);
		if (Item.IsAir)
		{
		    spriteBatch.Draw(_slotIcon.Value, position + (_slotBackground.Size() / 2f) * _scale, null, Color.White * 0.35f, 0f, _slotIcon.Size() / 2f, _scale, SpriteEffects.None, 0);
		}
		ItemSlot.DrawItemIcon(Item, _itemSlotContext, spriteBatch, GetDimensions().Center(), _scale * Item.scale, 32, Color.White);
	}
}