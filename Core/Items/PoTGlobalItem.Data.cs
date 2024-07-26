using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items;

// Managed static (per-type) data.

/// <summary>
///		Per-instance item data such as fields normally set in
///		<see cref="ModItem.SetDefaults"/>.
///	</summary>
public sealed class PoTInstanceItemData : GlobalItem
{
	public override bool InstancePerEntity => true;

	protected override bool CloneNewInstances => true;

	/// <summary>
	///		The type of item this is, not to be confused with
	///		<see cref="Item.type"/>.
	/// </summary>
	public ItemType ItemType { get; set; }

	/// <summary>
	///		The custom Path of Terraria-style <see cref="Core.Rarity"/>.
	/// </summary>
	public Rarity Rarity { get; set; }

	/// <summary>
	///		The influence of this item.
	/// </summary>
	public Influence Influence { get; set; }

	/// <summary>
	///		The formatted, post-rolled name containing formatting for rarities.
	/// </summary>
	public string SpecialName { get; set; } = string.Empty;

	/// <summary>
	///		The affixes of the item.
	/// </summary>
	public List<ItemAffix> Affixes { get; } = [];

	/// <summary>
	///		The amount of implicit affixes preceding rolled ones.
	/// </summary>
	internal int ImplicitCount { get; set; }

	internal int RealLevel { get; set; }

	public void SetItemLevel(Item item, int level)
	{
		IItemLevelControllerItem.SetLevel(item, level);
	}
}

/// <summary>
///		Per-type item data such as fields normally set in
///		<see cref="ModType.SetStaticDefaults"/>.
/// </summary>
public sealed class PoTStaticItemData
{
	/// <summary>
	///		The drop chance of this item.
	/// </summary>
	public float? DropChance { get; set; }

	/// <summary>
	///		Whether this item is <see cref="Rarity.Unique"/>.
	/// </summary>
	public bool IsUnique { get; set; }

	/// <summary>
	///		The minimum level this item needs to be to drop.
	/// </summary>
	public int MinDropItemLevel { get; set; }

	/// <summary>
	///		The item's description.
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	///		The item's description for alternate use (right-clicking).
	/// </summary>
	public string AltUseDescription { get; set; } = string.Empty;
}

partial class PoTGlobalItem
{
	private static Dictionary<int, PoTStaticItemData> _staticData = [];

	public override void Unload()
	{
		_staticData = null;
	}

	public static PoTStaticItemData GetStaticData(int type)
	{
		if (_staticData.TryGetValue(type, out PoTStaticItemData data))
		{
			return data;
		}

		return _staticData[type] = new PoTStaticItemData();
	}
}
