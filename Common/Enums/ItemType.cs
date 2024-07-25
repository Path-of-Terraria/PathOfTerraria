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

	Armor = Helmet | Chestplate | Leggings,
	Accessories = Ring | Charm,
	Equipment = Armor | Accessories,

	Melee = Sword | Spear,
	Magic = Staff | Tome | Wand,
	Ranged = Bow | Gun,
	Weapon = Melee | Magic | Ranged,

	AllGear = Equipment | Weapon,

	AllNoMap = AllGear | Jewel,

	All = AllNoMap | Map,
}