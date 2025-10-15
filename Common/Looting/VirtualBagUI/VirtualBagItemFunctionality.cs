using PathOfTerraria.Core.UI;
using SubworldLibrary;
using Terraria.GameContent.UI.States;

namespace PathOfTerraria.Common.Looting.VirtualBagUI;

internal class VirtualBagItemFunctionality : GlobalItem
{
	public static bool IsInAPlaceForPickup => SubworldSystem.Current is not null;

	public override bool ItemSpace(Item item, Player player)
	{
		return IsInAPlaceForPickup && VirtualBagStoragePlayer.VirtualBagAllowed(item, player);
	}

	public override bool OnPickup(Item item, Player player)
	{
		if (!IsInAPlaceForPickup || !VirtualBagStoragePlayer.VirtualBagAllowed(item, player))
		{
			return true;
		}

		player.GetModPlayer<VirtualBagStoragePlayer>().Storage.Add(item);

		AdvancedPopupRequest request = default;
		request.Velocity = new Vector2(0, -16);
		request.DurationInFrames = 120;
		request.Text = item.Name;
		request.Color = Color.MediumPurple;

		PopupText.NewText(request, item.Center);

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			(data.Value as VirtualBagUIState).RefreshStorage();
		}

		return false;
	}
}
