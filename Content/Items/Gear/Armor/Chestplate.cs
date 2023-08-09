using Terraria.ID;

namespace FunnyExperience.Content.Items.Gear.Armor
{
	[AutoloadEquip(EquipType.Body)]
	internal class Chestplate : Gear
	{
		public override string Texture => "Terraria/" + Terraria.GameContent.TextureAssets.Item[ItemID.IronChainmail].Name;

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
