using PathOfTerraria.Content.Projectiles.Magic;
using Terraria.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Magic
{
	internal class Staff : Gear
	{
		public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapon/Staff/ExampleStaff";

		public override void SetDefaults() {
			// DefaultToStaff handles setting various Item values that magic staff weapons use.
			// Hover over DefaultToStaff in Visual Studio to read the documentation!
			Item.DefaultToStaff(ModContent.ProjectileType<SparklingBall>(), 16, 25, 12);

			// Customize the UseSound. DefaultToStaff sets UseSound to SoundID.Item43, but we want SoundID.Item20
			Item.UseSound = SoundID.Item20;

			// Set damage and knockBack
			Item.SetWeaponValues(20, 5);

			// Set rarity and value
			Item.SetShopValues(ItemRarityColor.Green2, 10000);
		}
		
		public override string GenerateName()
		{
			string prefix = Main.rand.Next(5) switch
			{
				0 => "Arcane",
				1 => "Mystic",
				2 => "Eldritch",
				3 => "Luminous",
				4 => "Sacred",
				_ => "Unknown"
			};

			string suffix = Main.rand.Next(5) switch
			{
				0 => "Wrath",
				1 => "Whisper",
				2 => "Beacon",
				3 => "Veil",
				4 => "Echo",
				_ => "Unknown"
			};


			string item = GetType().ToString();
			return Rarity switch
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