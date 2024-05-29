using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class BurningRedBoots : Leggings
{
	public override float DropChance => 0.01f;
	public override bool IsUnique => true;
	public override List<GearAffix> GenerateImplicits()
	{
		return new List<GearAffix>() { (GearAffix)Affix.CreateAffix<MovementSpeed>(0.8f) };
	}

	public override List<GearAffix> GenerateAffixes()
	{
		return new List<GearAffix>() {
			(GearAffix)Affix.CreateAffix<MovementSpeed>(),
			(GearAffix)Affix.CreateAffix<MovementSpeed>(),
			(GearAffix)Affix.CreateAffix<MovementSpeed>()
		};
	}

	public override string GenerateName()
	{
		return $"[c/FF0000:Burning Red Boots]";
	}
	public override void PostRoll()
	{
		Item.defense = ItemLevel / 12;
	}
}