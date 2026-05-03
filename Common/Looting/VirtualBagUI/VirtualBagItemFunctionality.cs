using PathOfTerraria.Common.Looting.ItemFiltering;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using SubworldLibrary;
using Terraria.GameContent.UI.States;
using Terraria.ID;

namespace PathOfTerraria.Common.Looting.VirtualBagUI;

internal class VirtualBagItemFunctionality : GlobalItem, PostRoll.IGlobal
{
	/// <summary>
	///		Number of ticks after spawn before we evaluate the filter. The item flow goes
	///		<c>Item.NewItem(type)</c> → <c>SetDefaults</c> (random roll) → <c>netDefaults</c>
	///		(another <c>SetDefaults</c>) → <c>CopyNetStateTo</c> (which deserializes the intended
	///		data via <c>NetReceive</c>). Multiple of these can re-roll random data at <c>OnSpawn</c>
	///		time, so we wait a handful of ticks for the dust to settle before reading the rarity / item
	///		level / base type. This also lets the item visibly hit the ground, so the player sees that
	///		the filter is doing something rather than items vanishing mid-air.
	/// </summary>
	private const int EvaluationDelayTicks = 20;

	public override bool InstancePerEntity => true;

	public static bool IsInAPlaceForPickup => SubworldSystem.Current is not null;

	private bool _filterEvaluated;
	private int _ticksSinceSpawn;

	public override bool ItemSpace(Item item, Player player)
	{
		return IsInAPlaceForPickup && VirtualBagStoragePlayer.VirtualBagAllowed(item, player);
	}

	/// <summary>
	///		Fires after a roll, after <see cref="GlobalItem.LoadData"/>, and after
	///		<see cref="GlobalItem.NetReceive"/> — i.e. every time PoT-specific item data may have
	///		changed. Resetting the evaluation flag here means we only ever read rarity / level / base
	///		type once the data is stable.
	/// </summary>
	void PostRoll.IGlobal.PostRoll(Item item)
	{
		_filterEvaluated = false;
		_ticksSinceSpawn = 0;
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

		if (++_ticksSinceSpawn < EvaluationDelayTicks)
		{
			return;
		}

		_filterEvaluated = true;

		if (!VirtualBagStoragePlayer.VirtualBagAllowed(item, player))
		{
			return;
		}

		ItemFilter filter = player.GetModPlayer<ItemFilterPlayer>().ActiveFilter;

		if (filter is null || filter.Evaluate(item, out string reason))
		{
			return;
		}

		var storage = player.GetModPlayer<VirtualBagStoragePlayer>();
		storage.FilteredOutStorage.Add(item.Clone());
		storage.LastFilterName = filter.Name;

		LogFilterRejection(item, filter, "Update", reason);

		item.TurnToAir();

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			(data.Value as VirtualBagUIState).RefreshStorage();
		}
	}

	/// <summary>
	///		Surfaces what the filter actually saw for an item. Logged via chat so the player can verify
	///		the filter is reading the rarity / item type / level they expect, then via
	///		<see cref="Mod.Logger"/> for offline inspection.
	/// </summary>
	private static void LogFilterRejection(Item item, ItemFilter filter, string source, string reason)
	{
		string msg = $"[ItemFilter:{source}] '{filter.Name}' rejected '{item.Name}' — {reason}";

		Main.NewText(msg, Color.Orange);
		PoTMod.Instance?.Logger?.Info(msg);
	}

	public override bool OnPickup(Item item, Player player)
	{
		if (!IsInAPlaceForPickup || !VirtualBagStoragePlayer.VirtualBagAllowed(item, player))
		{
			return true;
		}

		var storage = player.GetModPlayer<VirtualBagStoragePlayer>();
		ItemFilter filter = player.GetModPlayer<ItemFilterPlayer>().ActiveFilter;

		if (filter is null || filter.Evaluate(item, out string reason))
		{
			storage.MatchedStorage.Add(item);

			if (filter is not null)
			{
				storage.LastFilterName = filter.Name;
			}
		}
		else
		{
			// Walk-over of an item that the deferred evaluation hasn't processed yet (e.g. the player
			// landed on it before the delay elapsed). Route through the bag the same way.
			storage.FilteredOutStorage.Add(item);
			storage.LastFilterName = filter.Name;
			LogFilterRejection(item, filter, "OnPickup", reason);
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
