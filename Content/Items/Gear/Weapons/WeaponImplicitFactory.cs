using System.Collections.Generic;
using System.IO;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons;

internal static class WeaponImplicitFactory
{
	public static ItemAffix Create(Item item)
	{
		PoTStaticItemData staticData = item.GetStaticData();
		if (!WeaponBaseTierRegistry.IsTiered(item) || staticData.IsUnique)
		{
			return null;
		}

		Type affixType = GetImplicitType(item.ResolveToSingleType(item.GetInstanceData().ItemType));
		if (affixType is null)
		{
			return null;
		}

		ItemAffixData affixData = AffixRegistry.GetItemData(affixType, item);
		int tierIndex = staticData.BaseTier - 1;
		if (tierIndex < 0 || tierIndex >= affixData.Tiers.Count)
		{
			throw new InvalidDataException($"Missing tier {staticData.BaseTier} for {affixType.Name} on {item.Name}.");
		}

		ItemAffixData.TierData tierData = affixData.Tiers[tierIndex];
		if (tierData.MinValue != tierData.MaxValue)
		{
			throw new InvalidDataException($"Weapon base implicit {affixType.Name} tier {staticData.BaseTier} must use one deterministic value.");
		}

		var affix = (ItemAffix)Affix.CreateAffix(affixType, tierData.MinValue);
		affix.Tier = tierIndex;
		return affix;
	}

	public static void EnsureImplicit(Item item)
	{
		ItemAffix expected = Create(item);
		if (expected is null)
		{
			return;
		}

		PoTInstanceItemData data = item.GetInstanceData();
		if (data.Affixes.Exists(affix => affix.GetType() == expected.GetType()))
		{
			return;
		}

		expected.IsImplicit = true;
		data.Affixes.Insert(Math.Clamp(data.ImplicitCount, 0, data.Affixes.Count), expected);
		data.ImplicitCount++;
	}

	private static Type GetImplicitType(ItemType itemType)
	{
		return itemType switch
		{
			ItemType.Sword => typeof(SwordCriticalMultiplierImplicitAffix),
			ItemType.Battleaxe => typeof(BattleaxeExecuteDamageImplicitAffix),
			ItemType.Bow => typeof(BowChargedShotDamageImplicitAffix),
			ItemType.Boomerang => typeof(BoomerangReturnDamageImplicitAffix),
			ItemType.Javelin => typeof(JavelinDashDamageImplicitAffix),
			ItemType.Staff => typeof(StaffChargeSpeedImplicitAffix),
			ItemType.Tome => typeof(TomeManaOnKillImplicitAffix),
			ItemType.Wand => typeof(WandFlurryDamageImplicitAffix),
			ItemType.WarShield => typeof(WarShieldCounterDamageImplicitAffix),
			ItemType.Whip => typeof(WhipTipDamageImplicitAffix),
			_ => null,
		};
	}
}

internal sealed class WeaponBaseImplicitGlobalItem : GlobalItem, GenerateImplicits.IGlobal
{
	void GenerateImplicits.IGlobal.ModifyImplicits(Item item, List<ItemAffix> implicits)
	{
		ItemAffix affix = WeaponImplicitFactory.Create(item);
		if (affix is not null && !implicits.Exists(existing => existing.GetType() == affix.GetType()))
		{
			implicits.Add(affix);
		}
	}
}
