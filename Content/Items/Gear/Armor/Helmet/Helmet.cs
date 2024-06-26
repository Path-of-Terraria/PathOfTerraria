using PathOfTerraria.Core;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class Helmet : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Helmet/Base";

	public override float DropChance => 1f;

	public override void Defaults()
	{
		ItemType = ItemType.Helmet;
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 10 + 1;
	}
	public override string GeneratePrefix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Adept",
			1 => "Tattered",
			2 => "Enchanted",
			3 => "Beautiful",
			4 => "Strange",
			_ => "Unknown"
		};
	}
	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Drape",
			1 => "Dome",
			2 => "Thought",
			3 => "Vision",
			4 => "Maw",
			_ => "Unknown"
		};
	}
}