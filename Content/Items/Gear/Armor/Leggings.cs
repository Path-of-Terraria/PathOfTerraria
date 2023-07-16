using Terraria.ID;

namespace FunnyExperience.Content.Items.Gear.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	internal class Leggings : Gear
	{
		public override string Texture => "Terraria/" + Terraria.GameContent.TextureAssets.Item[ItemID.IronGreaves].Name;

		public override void SetDefaults()
		{
			type = GearType.Leggings;
		}

		public override void PostRoll()
		{
			Item.defense = power / 14 + 1;
		}

		public override string GenerateName()
		{
			string prefix = Main.rand.Next(5) switch
			{
				0 => "Eagle",
				1 => "Swift",
				2 => "Feathered",
				3 => "Spiked",
				4 => "Buckled",
				_ => "Unknown"
			};

			string suffix = Main.rand.Next(5) switch
			{
				0 => "Sole",
				1 => "Soul",
				2 => "Paw",
				3 => "Talon",
				4 => "Claw",
				_ => "Unknown"
			};

			string item = Main.rand.Next(5) switch
			{
				0 => "Boots",
				1 => "Treads",
				2 => "Greaves",
				3 => "Tassets",
				4 => "Leggings",
				_ => "Unknown"
			};

			if (rarity == GearRarity.Normal)
				return item;

			if (rarity == GearRarity.Magic)
				return $"{prefix} {item}";

			if (rarity == GearRarity.Rare)
				return $"{prefix} {suffix} {item}";

			if (rarity == GearRarity.Unique)
				return Item.Name;

			return "Unknown Item";
		}
	}
}
