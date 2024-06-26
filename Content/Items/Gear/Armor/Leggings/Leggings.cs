using System.Collections.Generic;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Leggings : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Legs/Base";

	public override float DropChance => 1f;

	public override void Defaults()
	{
		ItemType = ItemType.Leggings;
	}
	public override List<ItemAffix> GenerateImplicits()
	{
		return new List<ItemAffix>() { (ItemAffix)Affix.CreateAffix<MovementSpeed>(0.8f) };
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