using Terraria.ID;

namespace FunnyExperience.Content.Items.Gear.Armor
{
	[AutoloadEquip(EquipType.Body)]
	internal class Chestplate : Gear
	{
		public override string Texture => $"{FunnyExperience.ModName}/Assets/Items/Gear/Armor/Body/Base";

		public override void SetDefaults()
		{
			type = GearType.Chestplate;
		}

		public override void PostRoll()
		{
			Item.defense = Power / 6 + 1;
		}

		public override string GenerateName()
		{
			string prefix = Main.rand.Next(6) switch
			{
				0 => "Enduring",
				1 => "Dreadknight's",
				2 => "Dragon",
				3 => "Golem",
				4 => "Warden's",
				5 => "Protected",
				_ => "Unknown"
			};

			string suffix = Main.rand.Next(5) switch
			{
				0 => "Core",
				1 => "Heart",
				2 => "Plate",
				3 => "Scale",
				4 => "Essence",
				_ => "Unknown"
			};

			string item = Main.rand.Next(3) switch
			{
				0 => "Breastplate",
				1 => "Body Armor",
				2 => "Chestpiece",
				_ => "Unknown"
			};
      
			return rarity switch
			{
				GearRarity.Normal => item,
				GearRarity.Magic => $"{prefix} {item}",
				GearRarity.Rare => $"{prefix} {suffix} {item}",
				GearRarity.Unique => Item.Name,
				_ => "Unknown Item"
			};
		}
	}
}
