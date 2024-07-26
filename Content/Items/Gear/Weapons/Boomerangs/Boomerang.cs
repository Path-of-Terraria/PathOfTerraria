using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal abstract class Boomerang : Gear, IItemLevelControllerItem
{
	public int ItemLevel
	{
		get => 1;
		set => this.GetInstanceData().RealLevel = value; // Technically preserves previous behavior.
	}

	protected virtual int BoomerangCount => 1;
	protected override string GearLocalizationCategory => "Boomerang";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetTextValue("Mods.PathOfTerraria.Gear.Boomerang.AltUse");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.CloneDefaults(ItemID.WoodenBoomerang);
		Item.width = 16;
		Item.height = 28;
		Item.useTime = 10;
		Item.useAnimation = 10;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.channel = true;
	}

	public override bool CanUseItem(Player player)
	{
		return player.ownedProjectileCounts[ModContent.ProjectileType<BoomerangProjectile>()] < BoomerangCount;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BoomerangProjectile>(), damage, knockback, player.whoAmI, player.altFunctionUse / 2, Type);

		if (player.altFunctionUse == 2)
		{
			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(180);
		}

		return false;
	}
}