using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

/// <summary>
/// Used to allow items to be inserted. If null, defaults to true
/// </summary>
internal delegate bool CanItemBeInsertedDelegate(Item newItem, Item currentItem);

internal class UICustomItemSlot : UIElement
{
	internal CanItemBeInsertedDelegate CanItemBeInserted = null;
	internal GetItemDelegate GetItem = null;
	internal SetItemDelegate SetItem = null;

	private Item _item = new(ItemID.Aglet);
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
		newItem ??= new Item();
		Item oldItem = _item;
		_item = newItem;
	}

	private void HandleInteraction()
	{
		Main.CurrentPlayer.mouseInterface = true;

		Item oldItem = _item.Clone();
		ItemSlot.Handle(ref _item, _itemSlotContext);
	}
	
	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		
		Main.NewText("Ishovering " + IsMouseHovering);
		Main.NewText("ignore interface " +  PlayerInput.IgnoreMouseInterface);
		Main.NewText("Can be inserted " + (CanItemBeInserted?.Invoke(Main.mouseItem, _item) != false));
		if (IsMouseHovering && !PlayerInput.IgnoreMouseInterface && CanItemBeInserted?.Invoke(Main.mouseItem, _item) != false)
		{
			Main.NewText("Hovering!!");
		    HandleInteraction();
		}

		Vector2 position = GetDimensions().ToRectangle().TopLeft();
		spriteBatch.Draw(_slotBackground.Value, position, null, _slotBackgroundColor, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0);
		if (_item.IsAir)
		{
		    spriteBatch.Draw(_slotIcon.Value, position + (_slotBackground.Size() / 2f) * _scale, null, Color.White * 0.35f, 0f, _slotIcon.Size() / 2f, _scale, SpriteEffects.None, 0);
		}
		ItemSlot.DrawItemIcon(_item, _itemSlotContext, spriteBatch, GetDimensions().Center(), _scale * _item.scale, 32, Color.White);
	}
}