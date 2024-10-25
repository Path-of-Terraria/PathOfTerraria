using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Projectiles.Magic;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal abstract class Wand : Gear
{
	protected override string GearLocalizationCategory => "Wand";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetText("Mods.PathOfTerraria.Gear.Wand.AltUse");

		Item.staff[Type] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 14;
		Item.width = Item.height = 40;
		Item.useTime = Item.useAnimation = 16;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Magic;
		Item.knockBack = 3;
		Item.UseSound = SoundID.Item20;
		Item.shootSpeed = 20f;
		Item.mana = 4;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.shoot = ModContent.ProjectileType<HomingProjectile>();
		Item.SetShopValues(ItemRarityColor.Green2, 50);

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Wand;
	}

	public override bool AltFunctionUse(Player player)
	{
		return !player.GetModPlayer<AltUsePlayer>().OnCooldown && player.CheckMana(Item.mana * 3, false, false);
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse == 2)
		{
			for (int i = 0; i < 4; ++i)
			{
				Vector2 adjSpeed = velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.9f, 1.1f);

				Projectile.NewProjectile(source, position, adjSpeed, type, damage, knockback, player.whoAmI);
			}

			player.CheckMana(Item.mana * 3, true, false);
			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(300);
		}

		return player.altFunctionUse != 2;
	}
}