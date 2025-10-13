using PathOfTerraria.Core.UI;
using System.Collections.Generic;

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
		storage[item.type] = Math.Min(storage[item.type] + item.stack, item.maxStack);

		AdvancedPopupRequest request = default;
		request.Velocity = new Vector2(0, -16);
		request.DurationInFrames = 120;
		request.Text = item.Name;
		request.Color = Color.LightBlue;

		PopupText.NewText(request, item.Center);

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(CurrencyPouchUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			(data.Value as CurrencyPouchUIState).Toggle();
			(data.Value as CurrencyPouchUIState).Toggle();
		}

		return false;
	}
}
