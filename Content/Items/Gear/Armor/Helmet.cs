using Terraria.ID;

namespace FunnyExperience.Content.Items.Gear.Armor
{
	[AutoloadEquip(EquipType.Head)]
	internal class Helmet : Gear
	{
		public override string Texture => "Terraria/" + Terraria.GameContent.TextureAssets.Item[ItemID.IronHelmet].Name;

		public override void SetDefaults()
		{
			type = GearType.Helmet;
		}

		public override void PostRoll()
		{
			Item.defense = power / 10 + 1;
		}

		public override string GenerateName()
		{
			string prefix = Main.rand.Next(5) switch
			{
				0 => "Adept",
				1 => "Tattered",
				2 => "Enchanted",
				3 => "Beautiful",
				4 => "Strange",
				_ => "Unknown"
			};

			string suffix = Main.rand.Next(5) switch
			{
				0 => "Drape",
				1 => "Dome",
				2 => "Thought",
				3 => "Vision",
				4 => "Maw",
				_ => "Unknown"
			};

			string item = Main.rand.Next(3) switch
			{
				0 => "Helmet",
				1 => "Visor",
				2 => "Crown",
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
