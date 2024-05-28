using PathOfTerraria.Content.Items.Gear.Affixes.ArmorAffixes;
using PathOfTerraria.Content.Items.Gear.Affixes;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Leggings : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Legs/Base";

	public override float DropChance => 1f;

	public override void SetDefaults()
	{
		GearType = GearType.Leggings;
	}
	public override List<GearAffix> GenerateImplicits()
	{
		return new List<GearAffix>() { GearAffix.CreateAffix<MovementSpeed>(-0.2f) };
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 12 + 1;
	}
	public override string GeneratePrefix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Eagle",
			1 => "Swift",
			2 => "Feathered",
			3 => "Spiked",
			4 => "Buckled",
			_ => "Unknown"
		};
	}
	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Boots",
			1 => "Treads",
			2 => "Greaves",
			3 => "Tassets",
			4 => "Leggings",
			_ => "Unknown"
		};
	}
}