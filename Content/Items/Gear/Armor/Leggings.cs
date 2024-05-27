namespace PathOfTerraria.Content.Items.Gear.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	internal class Leggings : Gear
	{
		public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Legs/Base";

		public override float DropChance => 1f;

		public override void SetDefaults()
		{
			GearType = GearType.Leggings;
		}

		public override void PostRoll()
		{
			Item.defense = ItemLevel / 14 + 1;
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

			if (Rarity == GearRarity.Normal)
				return item;

			if (Rarity == GearRarity.Magic)
				return $"{prefix} {item}";

			if (Rarity == GearRarity.Rare)
				return $"{prefix} {suffix} {item}";

			if (Rarity == GearRarity.Unique)
				return Item.Name;

			return "Unknown Item";
		}
	}
}
