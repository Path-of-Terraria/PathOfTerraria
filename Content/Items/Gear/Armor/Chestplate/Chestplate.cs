using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class Chestplate : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Body/Base";

	public override float DropChance => 1f;

	public override void Defaults()
	{
		GearType = GearType.Chestplate;
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 6 + 1;
	}
	public override string GeneratePrefix()
	{
		return Main.rand.Next(6) switch
		{
			0 => "Enduring",
			1 => "Dreadknight's",
			2 => "Dragon",
			3 => "Golem",
			4 => "Warden's",
			5 => "Protected",
			_ => "Unknown"
		};
	}
	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Core",
			1 => "Heart",
			2 => "Plate",
			3 => "Scale",
			4 => "Essence",
			_ => "Unknown"
		};
	}
}