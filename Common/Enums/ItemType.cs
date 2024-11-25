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

	Armor = Helmet | Chestplate | Leggings,
	Accessories = Ring | Charm,
	Equipment = Armor | Accessories,
	Offhand = Shield,

	Melee = Sword | Spear | MeleeFlail | WarShield | Battleaxe,
	Magic = Staff | Tome | Wand,
	Ranged = Bow | Gun | Boomerang | RangedFlail | Launcher | Javelin,
	Summoner = Whip | Grimoire,
	Weapon = Melee | Magic | Ranged | Whip,

	AllGear = Equipment | Weapon,

	AllNoMap = AllGear | Jewel,

	All = AllNoMap | Map,
}