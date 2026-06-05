using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Looting.ItemFiltering;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using SubworldLibrary;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;

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

	/// <summary>
	///		With <see cref="InstancePerEntity"/> on, this would otherwise allocate per-instance state
	///		for every <see cref="Item"/> in the world. Restrict to items the bag could ever care about
	///		so we don't pay that cost on blocks, ammo, materials, etc. The predicate mirrors
	///		<see cref="PoTGlobalItem.AppliesToEntity"/> — i.e. exactly the items that carry PoT data —
	///		using raw fields rather than <c>TryGetGlobalItem</c> because <c>AppliesToEntity</c> runs
	///		during <c>SetDefaults</c>, before global-type lookups are built.
	/// </summary>
	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		bool anyValidTrait = entity.damage > 0 || entity.defense > 0 || entity.accessory ||
		                     entity.headSlot > 0 || entity.bodySlot > 0 || entity.legSlot > 0 ||
		                     entity.ModItem is IPoTGlobalItem;

		return anyValidTrait && !entity.vanity && !entity.IsACoin;
	}

	public override bool ItemSpace(Item item, Player player)
	{
		return IsInAPlaceForPickup && VirtualBagStoragePlayer.VirtualBagAllowed(item, player);
	}

	/// <summary>
	///		Schedules a fresh filter evaluation. PoT's <c>PostRoll</c> hook fires after every path that
	///		mutates PoT-specific item data: <c>PoTItemHelper.Roll</c>, <c>LoadData</c>, and
	///		<c>NetReceive</c>. Each of those can change the values the filter reads (rarity, level, base
	///		type), so we clear our "already checked" state here and let <see cref="Update"/> re-run the
	///		filter once the data has settled. This does not directly route an item anywhere; the actual
	///		matched / filtered-out decision still lives in <see cref="Update"/>.
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

		if (filter is null || filter.Evaluate(item, out ItemFilterEvaluationResult result))
		{
			return;
		}

		var storage = player.GetModPlayer<VirtualBagStoragePlayer>();
		storage.FilteredOutStorage.Add(item.Clone());
		storage.LastFilterName = filter.Name;

		LogFilterRejection(item, filter, "Update", result.Reason);

		item.TurnToAir();

		if (Main.myPlayer == player.whoAmI && UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			(data.Value as VirtualBagUIState).RefreshStorage();
		}
	}

	/// <summary>
	///		Surfaces what the filter actually saw for an item via chat (filter, item, failed condition)
	///		and via <see cref="Mod.Logger"/>. Gated on <see cref="DeveloperConfig.LogFilterRejections"/>,
	///		which is itself only present in <c>DEBUG</c> builds — so this no-ops in release regardless.
	/// </summary>
	private static void LogFilterRejection(Item item, ItemFilter filter, string source, LocalizedText reason)
	{
		if (!DeveloperConfig.ShouldLogFilterRejections)
		{
			return;
		}

		string visible = Language.GetTextValue(
			"Mods.PathOfTerraria.UI.ItemFilter.RejectionChat",
			filter.Name,
			item.Name,
			reason?.Value ?? string.Empty);

		Main.NewText(visible, Color.Orange);
		// Logger gets the technical source tag (Update / OnPickup) for offline diagnosis.
		PoTMod.Instance?.Logger?.Info($"[ItemFilter:{source}] {visible}");
	}

	public override bool OnPickup(Item item, Player player)
	{
		if (!IsInAPlaceForPickup || !VirtualBagStoragePlayer.VirtualBagAllowed(item, player))
		{
			return true;
		}

		var storage = player.GetModPlayer<VirtualBagStoragePlayer>();
		ItemFilter filter = player.GetModPlayer<ItemFilterPlayer>().ActiveFilter;

		if (filter is null || filter.Evaluate(item, out ItemFilterEvaluationResult result))
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
			LogFilterRejection(item, filter, "OnPickup", result.Reason);
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
