using PathOfTerraria.Content.Projectiles.Magic;
using Terraria.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal class Staff : Gear
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Items/Gear/Weapons/Staff/ExampleStaff";
	public override float DropChance => 1f;

	protected override string GearLocalizationCategory => "Staff";

	public override void Defaults() {
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
}