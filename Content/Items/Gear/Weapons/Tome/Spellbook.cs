using PathOfTerraria.Common.Enums;
using PathOfTerraria.Content.Projectiles.Magic;
using PathOfTerraria.Core;
using Terraria.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Spellbook : Gear
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Items/Gear/Weapons/Tome/TomePlaceholder";
	public override float DropChance => 1f;

	protected override string GearLocalizationCategory => "Spellbook";

	public override void Defaults()
	{
		Item.damage = 10;
		Item.width = Item.height = 40;
		Item.useTime = Item.useAnimation = 20;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Magic;
		Item.knockBack = 1;
		Item.UseSound = SoundID.Item20;
		Item.shootSpeed = 25f;

		ItemType = ItemType.Wand;

		Item.shoot = ModContent.ProjectileType<TomeProjectile>();
		Item.SetShopValues(ItemRarityColor.Green2, 10000);
	}
}