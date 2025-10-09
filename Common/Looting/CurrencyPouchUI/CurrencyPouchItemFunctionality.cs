using PathOfTerraria.Common.Looting.CurrencyPouchUI;
using PathOfTerraria.Core.UI;

namespace PathOfTerraria.Common.Looting.VirtualBagUI;

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

		player.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType.TryAdd(item.type, 0);
		player.GetModPlayer<CurrencyPouchStoragePlayer>().StorageByType[item.type] += item.stack;

		AdvancedPopupRequest request = default;
		request.Velocity = new Vector2(0, -16);
		request.DurationInFrames = 120;
		request.Text = item.Name;
		request.Color = Color.LightBlue;

		PopupText.NewText(request, item.Center);

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(CurrencyPouchUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			//(data.Value as CurrencyPouchUIState).RefreshStorage();
		}

		return false;
	}
}
