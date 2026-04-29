using PathOfTerraria.Core.UI;
using System.Collections.Generic;
using PathOfTerraria.Common.Looting.VirtualBagUI;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchItemFunctionality : GlobalItem
{
	public override bool ItemSpace(Item item, Player player)
	{
		return CurrencyPouchStoragePlayer.CurrencyPouchAllowed(item);
	}

	public override bool OnPickup(Item item, Player player)
	{
		if (!CurrencyPouchStoragePlayer.CurrencyPouchAllowed(item))
		{
			return true;
		}

		Dictionary<int, int> storage = player.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType;
		storage.TryAdd(item.type, 0);
		int stored = Math.Min(item.stack, item.maxStack - storage[item.type]);

		if (stored <= 0)
		{
			return true;
		}

		string itemName = item.Name;
		Vector2 itemCenter = item.Center;
		storage[item.type] += stored;
		item.stack -= stored;
		player.GetModPlayer<VirtualBagStoragePlayer>().RecordCurrencyShard(item.type, stored);

		AdvancedPopupRequest request = default;
		request.Velocity = new Vector2(0, -16);
		request.DurationInFrames = 120;
		request.Text = itemName + $" ({stored})";
		request.Color = Color.LightBlue;

		PopupText.NewText(request, itemCenter);

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(CurrencyPouchUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			// Force a reload
			(data.Value as CurrencyPouchUIState).Toggle();
			(data.Value as CurrencyPouchUIState).Toggle();
		}

		if (item.stack <= 0)
		{
			item.TurnToAir();
			return false;
		}

		return true;
	}
}
