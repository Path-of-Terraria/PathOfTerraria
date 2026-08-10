using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Javelins;
using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using PathOfTerraria.Content.Items.Gear.Weapons.Tome;
using PathOfTerraria.Content.Items.Gear.Weapons.Wand;
using PathOfTerraria.Content.Items.Gear.Weapons.WarShields;
using PathOfTerraria.Content.Items.Gear.Weapons.Whip;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons;

/// <summary>
/// Defines progression tiers and bounded drop windows for exact, non-unique weapon bases.
/// Keeping the catalog centralized makes overlaps and family coverage auditable.
/// </summary>
internal static class WeaponBaseTierRegistry
{
	public const int MaximumBaseTier = 5;
	public const int MaximumSupportedItemLevel = 85;
	public const float TierWeightFactor = 1.5f;

	private static readonly HashSet<int> TieredItemIds = [];

	public static IReadOnlyCollection<int> AllTieredItemIds => TieredItemIds;

	public static void Apply()
	{
		TieredItemIds.Clear();

		// Swords
		Configure<WoodenSword>(1, 1, 8);
		Configure<StoneSword>(2, 1, 12);
		Configure<CopperBroadsword>(3, 5, 18);
		Configure<IronBroadsword>(4, 10, 28);
		Configure<Katana>(4, 15, 35);
		Configure<SteelBroadsword>(5, 20, MaximumSupportedItemLevel);

		// Battleaxes
		Configure<RustedBattleaxe>(1, 1, 16);
		Configure<IronBattleaxe>(3, 12, 28);
		Configure<SteelBattleaxe>(5, 18, MaximumSupportedItemLevel);

		// Bows
		Configure<BasicWoodenBow>(1, 1, 12);
		Configure<WoodenShortBow>(3, 5, 25);
		Configure<WoodenBow>(5, 12, MaximumSupportedItemLevel);

		// Boomerangs
		Configure<WoodenBoomerang>(1, 1, 7);
		Configure<StoneBoomerang>(2, 1, 10);
		Configure<CopperBoomerang>(3, 5, 14);
		Configure<IronBoomerang>(4, 8, 20);
		Configure<SilverBoomerang>(5, 14, MaximumSupportedItemLevel);

		// Javelins
		Configure<SharpenedStick>(1, 1, 10);
		Configure<IronPilum>(2, 5, 18);
		Configure<LeadDangpa>(3, 12, 24);
		Configure<IronAngon>(4, 12, 32);
		Configure<PlatinumGlaive>(5, 21, MaximumSupportedItemLevel);

		// Staves
		Configure<CopperStaff>(1, 1, 10);
		Configure<IronStaff>(2, 5, 18);
		Configure<LeadStaff>(3, 5, 22);
		Configure<SilverStaff>(3, 12, 28);
		Configure<GoldStaff>(4, 17, 40);
		Configure<TungstenStaff>(4, 12, 36);
		Configure<PlatinumStaff>(5, 25, MaximumSupportedItemLevel);

		// Tomes
		Configure<Spellbook>(1, 1, 30);
		Configure<Manuscript>(2, 20, 50);
		Configure<Lexicon>(4, 40, 70);
		Configure<Codex>(5, 60, MaximumSupportedItemLevel);

		// Wands
		Configure<WoodenWand>(1, 1, 10);
		Configure<MahoganyWand>(2, 5, 18);
		Configure<ShadewoodWand>(3, 12, 30);
		Configure<EbonwoodWand>(3, 12, 30);
		Configure<GemWand>(5, 17, MaximumSupportedItemLevel);

		// War shields
		Configure<WoodPlank>(1, 1, 10);
		Configure<StoneWarShield>(2, 5, 18);
		Configure<LeadBattleBulwark>(3, 12, 30);
		Configure<IronWarShield>(3, 12, 30);
		Configure<GoldenBattleBulwark>(4, 20, 40);
		Configure<PlatinumWarShield>(4, 20, 40);
		Configure<SteelWarShield>(5, 25, MaximumSupportedItemLevel);

		// Whips
		Configure<LeafWhip>(1, 1, 12);
		Configure<LeatherWhip>(2, 8, 20);
		Configure<BarbedLeatherWhip>(3, 13, 28);
		Configure<WebWhip>(4, 20, 36);
		Configure<ChainWhip>(5, 26, MaximumSupportedItemLevel);

		ValidateCoverage();
	}

	public static void Clear()
	{
		TieredItemIds.Clear();
	}

	public static bool IsTiered(Item item)
	{
		return item is not null && TieredItemIds.Contains(item.type) && item.GetStaticData().BaseTier > 0;
	}

	public static float GetTierWeight(int baseTier)
	{
		return MathF.Pow(TierWeightFactor, Math.Max(0, baseTier - 1));
	}

	private static void Configure<T>(int baseTier, int minimumItemLevel, int maximumItemLevel, float familyWeight = 1f)
		where T : ModItem
	{
		if (baseTier is < 1 or > MaximumBaseTier)
		{
			throw new InvalidDataException($"Weapon base tier must be between 1 and {MaximumBaseTier}: {typeof(T).Name}={baseTier}.");
		}

		int type = ModContent.ItemType<T>();
		PoTStaticItemData data = PoTGlobalItem.GetStaticData(type);
		data.BaseTier = baseTier;
		data.BaseFamilyWeight = familyWeight;
		data.SetDropItemLevelRange(minimumItemLevel, maximumItemLevel);
		TieredItemIds.Add(type);
	}

	private static void ValidateCoverage()
	{
		IEnumerable<IGrouping<ItemType, int>> families = TieredItemIds.GroupBy(type =>
		{
			Item item = ContentSamples.ItemsByType[type];
			return item.ResolveToSingleType(item.GetInstanceData().ItemType);
		});

		foreach (IGrouping<ItemType, int> family in families)
		{
			for (int level = 1; level <= MaximumSupportedItemLevel; level++)
			{
				if (!family.Any(type => PoTGlobalItem.GetStaticData(type).CanDropAtItemLevel(level)))
				{
					throw new InvalidDataException($"Tiered weapon family {family.Key} has no ordinary base at item level {level}.");
				}
			}

			float[] weights = [.. family.Select(type => PoTGlobalItem.GetStaticData(type).BaseFamilyWeight).Distinct()];
			if (weights.Length != 1 || weights[0] <= 0f)
			{
				throw new InvalidDataException($"Tiered weapon family {family.Key} must use one positive family weight.");
			}
		}
	}
}
