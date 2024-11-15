using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using System.Collections.Generic;
using Terraria.Localization;

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

	public override GlobalItem Clone(Item from, Item to)
	{
		var clone = base.Clone(from, to) as PoTInstanceItemData;
		clone.ItemType = ItemType;
		clone.Rarity = Rarity;
		clone.Influence = Influence;
		clone.SpecialName = SpecialName;
		clone.ImplicitCount = ImplicitCount;
		clone.RealLevel = RealLevel;
		clone.Affixes = Affixes;
		clone.Corrupted = Corrupted;
		clone.Cloned = Cloned;
		return clone;
	}

	/// <summary>
	///		The type of item this is, not to be confused with
	///		<see cref="Item.type"/>.
	/// </summary>
	public ItemType ItemType { get; set; }

	/// <summary>
	///		The custom Path of Terraria-style <see cref="Core.Rarity"/>.
	/// </summary>
	public ItemRarity Rarity { get; set; }

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
	public List<ItemAffix> Affixes { get; internal set; } = [];

	/// <summary>
	///		If the item is corrupt or not. Corrupted items cannot be modified with any currency shards.
	/// </summary>
	public bool Corrupted { get; set; }
	
	/// <summary>
	///		If the item is cloned or not. Cloned items cannot be modified with any currency shards.
	/// </summary>
	public bool Cloned { get; set; }

	/// <summary>
	///		The amount of implicit affixes preceding rolled ones.
	/// </summary>
	internal int ImplicitCount { get; set; }

	internal int RealLevel { get; set; }
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
	public LocalizedText Description { get; set; } = LocalizedText.Empty;

	/// <summary>
	///		The item's description for alternate use (right-clicking).
	/// </summary>
	public LocalizedText AltUseDescription { get; set; } = LocalizedText.Empty;
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
