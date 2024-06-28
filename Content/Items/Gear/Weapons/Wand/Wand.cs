using PathOfTerraria.Content.Projectiles.Magic;
using PathOfTerraria.Core;
using Terraria.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal class Wand : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Wand/WandPlaceholder";
	public override float DropChance => 1f;

	protected override string GearLocalizationCategory => "Wand";

	public override void Defaults()
	{
		Item.damage = 14;
		Item.width = Item.height = 40;
		Item.useTime = Item.useAnimation = 16;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Magic;
		Item.knockBack = 3;
		Item.UseSound = SoundID.Item20;
		Item.shootSpeed = 20f;

		ItemType = ItemType.Wand;

		Item.shoot = ModContent.ProjectileType<HomingProjectile>();
		Item.SetShopValues(ItemRarityColor.Green2, 10000);
	}
}