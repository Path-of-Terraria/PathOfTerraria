using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Items.Gear.Weapons.Wand;
using PathOfTerraria.Content.Skills.Magic;
using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Content.Skills.Ranged;
using PathOfTerraria.Content.Skills.Summon;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Classing;

/// <summary> A starting class' identifier. </summary>
public enum StarterClass : sbyte
{
	None = -1,
	Melee = 0,
	Ranged,
	Magic,
	Summon,
	Count,
}

/// <summary> Stores information about a given starter class. </summary>
internal record struct StarterClassInfo()
{
	public required int WeaponItemId;
	public required Type[] SkillTypes = [];
}

public static class StarterClassExtensions
{
	public static LocalizedText Localize(this StarterClass classes)
	{
		return Language.GetText("Mods.PathOfTerraria.UI.ClassPages.StarterClasses." + classes.ToString());
	}

	public static string GetNouns(this StarterClass classes)
	{
		return Language.GetTextValue("Mods.PathOfTerraria.UI.ClassPages." + classes.ToString() + ".Nouns");
	}
}

internal sealed class StarterClasses : ModSystem
{
	private static readonly StarterClassInfo[] infoByClass = new StarterClassInfo[(int)StarterClass.Count];

	public override void PostSetupContent()
	{
		infoByClass[(int)StarterClass.Melee] = new()
		{
			WeaponItemId = ItemID.CopperBroadsword,
			SkillTypes = [typeof(Berserk)],
		};
		infoByClass[(int)StarterClass.Ranged] = new()
		{
			WeaponItemId = ModContent.ItemType<WoodenShortBow>(),
			SkillTypes = [typeof(RainOfArrows)],
		};
		infoByClass[(int)StarterClass.Magic] = new()
		{
			WeaponItemId = ModContent.ItemType<MahoganyWand>(),
			SkillTypes = [typeof(Fireball)],
		};
		infoByClass[(int)StarterClass.Summon] = new()
		{
			WeaponItemId = ItemID.SlimeStaff,
			SkillTypes = [typeof(FlameSage)],
		};
	}

	public static StarterClassInfo GetInfo(StarterClass id)
	{
		return infoByClass[(int)id];
	}
}