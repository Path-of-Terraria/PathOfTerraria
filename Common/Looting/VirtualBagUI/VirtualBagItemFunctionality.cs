using PathOfTerraria.Common.Looting.ItemFiltering;
using PathOfTerraria.Core.UI;
using SubworldLibrary;
using Terraria.GameContent.UI.States;
using Terraria.ID;

namespace PathOfTerraria.Common.Looting.VirtualBagUI;

internal class VirtualBagItemFunctionality : GlobalItem
{
	public override bool InstancePerEntity => true;

	public static bool IsInAPlaceForPickup => SubworldSystem.Current is not null;

	/// <summary>
	///		Whether this world item has already been checked against the active filter. Item.NewItem
	///		fires OnSpawn before <c>CopyNetStateTo</c> has run, so PoT-specific data like
	///		<c>PoTInstanceItemData.Rarity</c> is still default at OnSpawn time. We defer the filter check
	///		to the first Update tick, by which point the netstate has been copied across.
	/// </summary>
	private bool _filterEvaluated;

	public override bool ItemSpace(Item item, Player player)
	{
		return IsInAPlaceForPickup && VirtualBagStoragePlayer.VirtualBagAllowed(item, player);
	}

	public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
	{
		if (_filterEvaluated || Main.netMode == NetmodeID.Server)
		{
			return;
		}

		if (Main.LocalPlayer is not { active: true } player)
		{
			return;
		}

		_filterEvaluated = true;

		if (!VirtualBagStoragePlayer.VirtualBagAllowed(item, player))
		{
			return;
		}

		ItemFilter filter = player.GetModPlayer<ItemFilterPlayer>().ActiveFilter;

		if (filter is null || filter.Matches(item))
		{
			return;
		}

		var storage = player.GetModPlayer<VirtualBagStoragePlayer>();
		storage.FilteredOutStorage.Add(item.Clone());
		storage.LastFilterName = filter.Name;
		item.TurnToAir();

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			(data.Value as VirtualBagUIState).RefreshStorage();
		}
	}

	public override bool OnPickup(Item item, Player player)
	{
		if (!IsInAPlaceForPickup || !VirtualBagStoragePlayer.VirtualBagAllowed(item, player))
		{
			return true;
		}

		var storage = player.GetModPlayer<VirtualBagStoragePlayer>();
		ItemFilter filter = player.GetModPlayer<ItemFilterPlayer>().ActiveFilter;

		if (filter is null || filter.Matches(item))
		{
			storage.MatchedStorage.Add(item);

			if (filter is not null)
			{
				storage.LastFilterName = filter.Name;
			}
		}
		else
		{
			// Non-matching items normally get intercepted in OnSpawn so they never drop visibly. Picking
			// one up still happens for items that pre-existed before the filter was set.
			storage.FilteredOutStorage.Add(item);
			storage.LastFilterName = filter.Name;
		}

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
