using PathOfTerraria.Content.Projectiles.Magic;
using Terraria.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Magic
{
	internal class Wand : Gear
	{
		public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapon/Staff/WandPlaceholder";

		public override float DropChance => 1f;

		public override void SetDefaults() {

			Item.damage = 14;
			Item.width = Item.height = 40;
			Item.useTime = Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.autoReuse = true;
			Item.DamageType = DamageClass.Magic;
			Item.knockBack = 3;
			Item.UseSound = SoundID.Item20;
			Item.shootSpeed = 20f;

			GearType = GearType.Wand;

			Item.shoot = ModContent.ProjectileType<HomingProjectile>();

			Item.SetShopValues(ItemRarityColor.Green2, 10000);
		}
		
		public override string GenerateName()
		{
			// didnt change this, is the same as wand
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