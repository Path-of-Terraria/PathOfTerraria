using PathOfTerraria.Common.Enums;
using PathOfTerraria.Content.Projectiles.Magic;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal class Wand : Gear
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Items/Gear/Weapons/Wand/WandPlaceholder";

	protected override string GearLocalizationCategory => "Wand";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 14;
		Item.width = Item.height = 40;
		Item.useTime = Item.useAnimation = 16;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Magic;
		Item.knockBack = 3;
		Item.UseSound = SoundID.Item20;
		Item.shootSpeed = 20f;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Wand;

		Item.shoot = ModContent.ProjectileType<HomingProjectile>();
		Item.SetShopValues(ItemRarityColor.Green2, 10000);
	}
}