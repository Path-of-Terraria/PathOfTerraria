using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Wand;
using PathOfTerraria.Content.Skills.Magic;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Content.Skills.Ranged;
using PathOfTerraria.Content.Skills.Summon;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Classing;

/// <summary>
/// Stores starter info for a given class. These are automatically applied in <see cref="Systems.VanillaModifications.AddClassSelectInPlayer"/>.
/// </summary>
internal readonly record struct StarterClassInfo(int WeaponItemId, Type SkillType)
{
	public static Dictionary<StarterClasses, StarterClassInfo> InfoByClass = new()
	{
		{ StarterClasses.Melee, new StarterClassInfo(ItemID.CopperBroadsword, typeof(Berserk)) },
		{ StarterClasses.Ranged, new StarterClassInfo(ModContent.ItemType<WoodenShortBow>(), typeof(RainOfArrows)) },
		{ StarterClasses.Magic, new StarterClassInfo(ModContent.ItemType<MahoganyWand>(), typeof(Fireball)) },
		{ StarterClasses.Summon, new StarterClassInfo(ItemID.SlimeStaff, typeof(FlameSage)) },
	};
}
