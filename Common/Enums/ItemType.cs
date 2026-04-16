using System.Numerics;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Enums;

[Flags]
public enum ItemType : long
{
	None = 0,
	Sword = 1 << 0,
	Spear = 1 << 1,
	Bow = 1 << 2,
	Gun = 1 << 3,
	Staff = 1 << 4,
	Tome = 1 << 5,
	Helmet = 1 << 6,
	Chestplate = 1 << 7,
	Leggings = 1 << 8,
	Ring = 1 << 9,
	Charm = 1 << 10,
	Wand = 1 << 11,
	Jewel = 1 << 12,
	Map = 1 << 13,
	Boomerang = 1 << 14,
	MeleeFlail = 1 << 15,
	RangedFlail = 1 << 16,
	Launcher = 1 << 17,
	Javelin = 1 << 18,
	Whip = 1 << 19,
	WarShield = 1 << 20,
	Grimoire = 1 << 21,
	Battleaxe = 1 << 22,
	Amulet = 1 << 23,
	Shield = 1 << 24,
	Quiver = 1 << 25,
	Talisman = 1 << 26,
	Focus = 1 << 27,
	Summon = 1 << 28,
	TypeCount = 29,

	Armor = Helmet | Chestplate | Leggings,
	Accessories = Ring | Charm | Amulet,
	Equipment = Armor | Accessories,
	Offhand = Shield | Quiver | Talisman | Focus,

	Melee = Sword | Spear | MeleeFlail | WarShield | Battleaxe,
	Magic = Staff | Tome | Wand,
	Ranged = Bow | Gun | Boomerang | RangedFlail | Launcher | Javelin,
	Summoner = Whip | Grimoire | Summon,
	MeleeOrRanged = Melee | Ranged,
	Weapon = Melee | Magic | Ranged | Whip,

	AllGear = Equipment | Weapon,

	AllNoMap = AllGear | Jewel,

	All = AllNoMap | Map,
}

public static class ItemTypeLocalization
{
	public static string LocalizeText(this ItemType type)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Gear.{type}.Name");
	}
}

public static class ItemTypeExtensions
{
	public static bool IsSingleType(this ItemType type)
	{
		return type != ItemType.None && BitOperations.PopCount((ulong)type) == 1;
	}

	public static ItemType ResolveToSingleType(this Item item, ItemType type)
	{
		if (type.IsSingleType())
		{
			return type;
		}

		if ((type & ItemType.Armor) != ItemType.None)
		{
			if (item.headSlot > 0)
			{
				return ItemType.Helmet;
			}

			if (item.bodySlot > 0)
			{
				return ItemType.Chestplate;
			}

			if (item.legSlot > 0)
			{
				return ItemType.Leggings;
			}
		}

		if ((type & ItemType.Offhand) != ItemType.None)
		{
			if (item.shieldSlot > 0)
			{
				return ItemType.Shield;
			}

			if ((type & ItemType.Quiver) != ItemType.None)
			{
				return ItemType.Quiver;
			}

			if ((type & ItemType.Talisman) != ItemType.None)
			{
				return ItemType.Talisman;
			}

			if ((type & ItemType.Focus) != ItemType.None)
			{
				return ItemType.Focus;
			}
		}

		if ((type & ItemType.Accessories) != ItemType.None)
		{
			return ItemType.Charm;
		}

		if (item.damage > 0)
		{
			if (item.CountsAsClass<SummonMeleeSpeedDamageClass>() && item.shoot > ProjectileID.None)
			{
				return ItemType.Whip;
			}

			if (item.CountsAsClass<SummonDamageClass>())
			{
				return ItemType.Grimoire;
			}

			if (item.CountsAsClass<MagicDamageClass>())
			{
				return ItemType.Staff;
			}

			if (item.CountsAsClass<RangedDamageClass>())
			{
				if (item.ammo == AmmoID.Arrow || item.useAmmo == AmmoID.Arrow)
				{
					return ItemType.Bow;
				}

				if (item.ammo == AmmoID.Rocket || item.useAmmo == AmmoID.Rocket)
				{
					return ItemType.Launcher;
				}

				if (item.shoot > ProjectileID.None)
				{
					int aiStyle = ContentSamples.ProjectilesByType[item.shoot].aiStyle;

					if (aiStyle == ProjAIStyleID.Boomerang)
					{
						return ItemType.Boomerang;
					}

					if (aiStyle == ProjAIStyleID.Flail)
					{
						return ItemType.RangedFlail;
					}

					if (aiStyle == ProjAIStyleID.Spear || item.consumable)
					{
						return ItemType.Javelin;
					}
				}

				return ItemType.Gun;
			}

			if (item.CountsAsClass<MeleeDamageClass>())
			{
				if (!item.noMelee && item.axe > 0)
				{
					return ItemType.Battleaxe;
				}

				if (item.shoot > ProjectileID.None)
				{
					int aiStyle = ContentSamples.ProjectilesByType[item.shoot].aiStyle;

					if (aiStyle == ProjAIStyleID.Spear)
					{
						return ItemType.Spear;
					}

					if (aiStyle == ProjAIStyleID.Flail)
					{
						return ItemType.MeleeFlail;
					}

					if (aiStyle == ProjAIStyleID.Boomerang)
					{
						return ItemType.Boomerang;
					}
				}

				return ItemType.Sword;
			}
		}

		return GetFirstSetFlag(type);
	}

	private static ItemType GetFirstSetFlag(ItemType type)
	{
		ulong raw = (ulong)type;
		if (raw == 0)
		{
			return ItemType.None;
		}

		int firstBit = BitOperations.TrailingZeroCount(raw);
		return (ItemType)(1ul << firstBit);
	}
}
