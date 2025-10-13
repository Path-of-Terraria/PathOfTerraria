using Terraria.UI;

namespace PathOfTerraria.Common.UI.Elements;

public class NoBGItemSlot : UIElement
{
	public Item Item => _itemArray[_itemIndex];

	private readonly Item[] _itemArray;
	private readonly int _itemIndex;
	private readonly int _itemSlotContext;

	public NoBGItemSlot(Item[] itemArray, int itemIndex, int itemSlotContext)
	{
		_itemArray = itemArray;
		_itemIndex = itemIndex;
		_itemSlotContext = itemSlotContext;
		Width = new StyleDimension(48f, 0f);
		Height = new StyleDimension(48f, 0f);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (IsMouseHovering)
		{
			Main.LocalPlayer.mouseInterface = true;
			Item inv = _itemArray[_itemIndex];
			ItemSlot.OverrideHover(ref inv, _itemSlotContext);
			ItemSlot.LeftClick(ref inv, _itemSlotContext);
			ItemSlot.RightClick(ref inv, _itemSlotContext);
			ItemSlot.MouseHover(ref inv, _itemSlotContext);
			_itemArray[_itemIndex] = inv;
		}

		Vector2 position = GetDimensions().Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
		ItemSlot.Draw(Main.spriteBatch, _itemArray, ItemSlot.Context.MouseItem, _itemIndex, position, Color.White);
	}
}