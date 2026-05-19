using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Core.Items;
using SubworldLibrary;

namespace PathOfTerraria.Common.ItemDropping;

/// <summary>
/// Helpers for populating chests in mapping/boss subworlds with loot that feels worth opening.
/// Applies a baseline rarity boost, includes any active map rarity affixes, and biases affix
/// tier rolls toward higher tiers via <see cref="SmartLoot"/>.
/// </summary>
internal static class MapChestLoot
{
	/// <summary> Flat rarity boost applied to every map chest, on top of map affixes. </summary>
	public const float BaseRarityBoost = 0.5f;

	/// <summary> Number of rolled mob-style drops to place per chest (was 3 prior to the chest rework). </summary>
	public const int MobDropCount = 5;

	/// <summary> Smart-loot tier bias multiplier per tier index. 0.5 means tier N is rolled at 1.5^N weight. </summary>
	public const float SmartLootTierBias = 0.5f;

	/// <summary>
	/// Rolls the standard chest mob-drop loot pool with the chest rarity boost applied.
	/// Must be called inside a <see cref="SmartLoot.Begin"/> scope so the resulting <c>new Item</c>
	/// affix rolls inherit the tier bias.
	/// </summary>
	public static List<ItemDatabase.ItemRecord> RollMobDrops(int count = MobDropCount)
	{
		float rarity = 1f + BaseRarityBoost;

		if (SubworldSystem.Current is MappingWorld)
		{
			rarity += ArpgNPC.DomainRarityBoost();
		}

		return DropTable.RollManyMobDrops(count, PoTItemHelper.PickItemLevel(), rarity, random: WorldGen.genRand);
	}

	/// <summary>
	/// Builds a chest <see cref="Item"/> from a rolled <see cref="ItemDatabase.ItemRecord"/>, applying the
	/// record's rolled rarity and re-running the affix roll so the chest item actually reflects the gear pool's
	/// rarity selection. The default <c>new Item(type)</c> path leaves <see cref="PoTInstanceItemData.Rarity"/>
	/// at <see cref="ItemRarity.Normal"/>, which silently produces zero affixes regardless of what the gear pool rolled.
	/// </summary>
	public static Item BuildChestItem(ItemDatabase.ItemRecord record)
	{
		var item = new Item(record.ItemId, record.Item.stack);

		if (item.TryGetGlobalItem<PoTInstanceItemData>(out _))
		{
			PoTInstanceItemData data = item.GetInstanceData();
			PoTStaticItemData staticData = item.GetStaticData();

			data.Rarity = staticData.IsUnique ? ItemRarity.Unique : record.Rarity;
			PoTItemHelper.Roll(item, PoTItemHelper.PickItemLevel());
		}

		return item;
	}
}

/// <summary>
/// Scoped affix tier bias. While a scope is active, <see cref="Common.Data.Models.ItemAffixData.GetAppropriateTierData"/>
/// re-weights eligible tiers as <c>weight * (1 + Bias)^tierIndex</c>, pushing rolls toward higher tiers.
/// </summary>
internal static class SmartLoot
{
	private static int _depth;
	private static float _bias;

	/// <summary> Current tier bias, or 0 if no scope is active. </summary>
	public static float TierBias => _depth > 0 ? _bias : 0f;

	public static Scope Begin(float bias = MapChestLoot.SmartLootTierBias)
	{
		// Outer scope wins — nested calls do not overwrite an active bias, since chest population
		// can transitively trigger item rolls that themselves run through the same code paths.
		if (_depth == 0)
		{
			_bias = bias;
		}

		_depth++;
		return new Scope();
	}

	internal readonly struct Scope : IDisposable
	{
		public void Dispose()
		{
			_depth--;

			if (_depth == 0)
			{
				_bias = 0f;
			}
		}
	}
}
