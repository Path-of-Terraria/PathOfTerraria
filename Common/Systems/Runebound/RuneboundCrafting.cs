using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Affixes.MobTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Common.Systems.Runebound;

internal enum RuneboundFamily : byte
{
	Vigor,
	Bastion,
	Embers,
	Rime,
	Tempests,
	Void,
	Might,
	Precision,
	Haste,
	Spirit,
}

internal enum RunestoneGrade : byte
{
	Faint,
	Greater,
	Perfect,
}

internal static class RuneboundCrafting
{
	public static Type GetMobAffixType(RuneboundFamily family)
	{
		return family switch
		{
			RuneboundFamily.Vigor => typeof(DoubleLife),
			RuneboundFamily.Bastion => typeof(StalwartAffix),
			RuneboundFamily.Embers => typeof(FlamingAffix),
			RuneboundFamily.Rime => typeof(ColdAffix),
			RuneboundFamily.Tempests => typeof(LightningAffix),
			RuneboundFamily.Void => typeof(ChaoticAffix),
			RuneboundFamily.Might => typeof(DamageAffix),
			RuneboundFamily.Precision => typeof(AggravatorAffix),
			RuneboundFamily.Haste => typeof(FastAffix),
			RuneboundFamily.Spirit => typeof(SiphonerAffix),
			_ => null,
		};
	}

	public static Type GetAffixType(RuneboundFamily family, Item item)
	{
		ItemType itemType = item.ResolveToSingleType(item.GetInstanceData().ItemType);

		return family switch
		{
			RuneboundFamily.Vigor when HasAny(itemType, ItemType.Armor | ItemType.Shield | ItemType.Ring | ItemType.Amulet)
				=> typeof(BaseLifeAffix),

			RuneboundFamily.Bastion when itemType == ItemType.Shield => typeof(IncreaseBlockAffix),
			RuneboundFamily.Bastion when HasAny(itemType, ItemType.Armor) => typeof(DefenseItemAffix),

			RuneboundFamily.Embers when HasAny(itemType, ItemType.Weapon | ItemType.Quiver) => typeof(ExtraFireDamage),
			RuneboundFamily.Embers when HasAny(itemType, ItemType.Armor | ItemType.Ring | ItemType.Amulet) => typeof(FireResistItemAffix),

			RuneboundFamily.Rime when HasAny(itemType, ItemType.Weapon | ItemType.Quiver) => typeof(ExtraColdDamage),
			RuneboundFamily.Rime when HasAny(itemType, ItemType.Armor | ItemType.Ring | ItemType.Amulet) => typeof(ColdResistItemAffix),

			RuneboundFamily.Tempests when HasAny(itemType, ItemType.Weapon | ItemType.Quiver) => typeof(ExtraLightningDamage),
			RuneboundFamily.Tempests when HasAny(itemType, ItemType.Armor | ItemType.Ring | ItemType.Amulet) => typeof(LightningResistItemAffix),

			RuneboundFamily.Void when itemType == ItemType.Grimoire => typeof(ChanceToApplyPoisonItemAffix),
			RuneboundFamily.Void when HasAny(itemType, ItemType.Weapon | ItemType.Quiver) => typeof(ExtraChaosDamage),
			RuneboundFamily.Void when HasAny(itemType, ItemType.Armor | ItemType.Ring | ItemType.Amulet) => typeof(ChaosResistItemAffix),

			RuneboundFamily.Might when HasAny(itemType, ItemType.Summoner) => typeof(IncreasedSummonDamageAffix),
			RuneboundFamily.Might when HasAny(itemType, ItemType.Melee) => typeof(IncreasedMeleeDamageAffix),
			RuneboundFamily.Might when HasAny(itemType, ItemType.Ranged) => typeof(IncreasedRangedDamageAffix),
			RuneboundFamily.Might when HasAny(itemType, ItemType.Magic) => typeof(IncreasedMagicDamageAffix),

			RuneboundFamily.Precision when HasAny(itemType, ItemType.Quiver | ItemType.Focus) => typeof(FlatCriticalStrikeChanceAffix),
			RuneboundFamily.Precision when HasAny(itemType, ItemType.Weapon | ItemType.Offhand | ItemType.Summoner)
				=> typeof(PercentageIncreasedCriticalStrikeChanceAffix),

			RuneboundFamily.Haste when itemType == ItemType.Wings => typeof(WingFlightTimeAffix),
			RuneboundFamily.Haste when itemType == ItemType.JumpAccessories => typeof(JumpSpeedAffix),
			RuneboundFamily.Haste when HasAny(itemType, ItemType.Armor) => typeof(MovementSpeedAffix),
			RuneboundFamily.Haste when HasAny(itemType, ItemType.Melee | ItemType.Shield | ItemType.Quiver)
				=> typeof(IncreasedAttackSpeedAffix),

			RuneboundFamily.Spirit when HasAny(itemType, ItemType.Ring | ItemType.Amulet) => typeof(ManaAffix),
			RuneboundFamily.Spirit when HasAny(itemType, ItemType.Weapon | ItemType.Summoner | ItemType.Armor | ItemType.Talisman)
				=> typeof(ManaRegenAffix),
			_ => null,
		};
	}

	private static bool HasAny(ItemType value, ItemType mask)
	{
		return (value & mask) != ItemType.None;
	}
}

/// <summary>Single-threaded spawn context consumed by <see cref="MobSystem.ArpgNPC.SetDefaults"/>.</summary>
internal static class RuneboundSpawnContext
{
	private static RuneboundFamily? pendingFamily;

	public static void Prepare(RuneboundFamily family)
	{
		pendingFamily = family;
	}

	public static bool TryConsume(out RuneboundFamily family)
	{
		if (pendingFamily is not { } value)
		{
			family = default;
			return false;
		}

		family = value;
		pendingFamily = null;
		return true;
	}

	public static void Clear()
	{
		pendingFamily = null;
	}
}
