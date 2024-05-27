﻿namespace PathOfTerraria.Content.Items.Gear;

 [Flags]
 public enum GearType : long
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

	 Armor = Helmet | Chestplate | Leggings,
	 Accessories = Ring | Charm,
	 Equipment = Armor | Accessories,

	 Melee = Sword | Spear,
	 Magic = Staff | Tome | Wand,
	 Ranged = Bow | Gun,
	 Weapon = Melee | Magic | Ranged,
 }

 public enum GearRarity
 {
	 Normal,
	 Magic,
	 Rare,
	 Unique
 }

 public enum GearInfluence
 {
	 None,
	 Solar,
	 Lunar
 }

 public enum ModifierType
 {
	 Passive,
	 Added,
	 Multiplier
 }
	
 public enum MobRarity
 {
	 Normal = 0,
	 Magic = 1,
	 Rare = 2,
	 Unique = 3
 }