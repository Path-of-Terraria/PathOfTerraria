#if DEBUG || STAGING
using PathOfTerraria.Content.Items.Currency;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.ItemCreator;

internal class InfiniteShardSlot : UIElement
{
	private readonly int _shardItemType;
	private readonly Item _displayItem;
	private readonly Func<Item> _getEditItem;
	private readonly Action _onShardApplied;

	private static Asset<Texture2D>? _background;

	public InfiniteShardSlot(int shardItemType, Func<Item> getEditItem, Action onShardApplied)
	{
		_shardItemType = shardItemType;
		_getEditItem = getEditItem;
		_onShardApplied = onShardApplied;

		_displayItem = new Item();
		_displayItem.SetDefaults(shardItemType);

		Width.Set(36, 0);
		Height.Set(36, 0);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		_background ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/ItemSlot");

		CalculatedStyle dims = GetDimensions();
		Vector2 center = dims.Center();

		// Draw a normal item slot frame so the shard bar matches standard slot visuals.
		Texture2D bgTex = _background.Value;
		spriteBatch.Draw(bgTex, center, null, Color.White, 0f, bgTex.Size() / 2f, 1f, SpriteEffects.None, 0);

		Main.instance.LoadItem(_displayItem.type);
		ItemSlot.DrawItemIcon(_displayItem, ItemSlot.Context.InventoryItem, spriteBatch, center, 1f, 24f, Color.White);

		if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface)
		{
			return;
		}

		Main.HoverItem = _displayItem.Clone();
		Main.hoverItemName = _displayItem.Name;
		Main.LocalPlayer.mouseInterface = true;

		HandleClicks();
	}

	private void HandleClicks()
	{
		if (Main.mouseLeft && Main.mouseLeftRelease)
		{
			Item editItem = _getEditItem();

			if (editItem is { IsAir: false } && _displayItem.ModItem is CurrencyShard shard)
			{
				if (shard.CanUseInPouch(editItem, out _))
				{
					shard.ApplyToItem(editItem);
					_onShardApplied?.Invoke();
				}
			}

			Main.mouseLeftRelease = false;
		}

		if (Main.mouseRight && Main.mouseRightRelease)
		{
			Item editItem = _getEditItem();

			if (editItem is { IsAir: false } && _displayItem.ModItem is CurrencyShard shard)
			{
				if (shard.CanUseInPouch(editItem, out _))
				{
					shard.ApplyToItem(editItem);
					_onShardApplied?.Invoke();
				}
			}

			Main.mouseRightRelease = false;
		}
	}
}
#endif
