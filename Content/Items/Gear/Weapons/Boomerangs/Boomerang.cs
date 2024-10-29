using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal abstract class Boomerang : Gear, GetItemLevel.IItem
{
	int GetItemLevel.IItem.GetItemLevel(int realLevel)
	{
		return 1;
	}

	protected virtual int BoomerangCount => 1;
	protected override string GearLocalizationCategory => "Boomerang";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetText("Mods.PathOfTerraria.Gear.Boomerang.AltUse");
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

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Boomerang;
	}

	public override bool CanUseItem(Player player)
	{
		return player.ownedProjectileCounts[ModContent.ProjectileType<BoomerangProjectile>()] < BoomerangCount;
	}

	public override bool AltFunctionUse(Player player)
	{
		return !player.GetModPlayer<AltUsePlayer>().OnCooldown;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse == 2)
		{
			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(180);
			damage = (int)(damage * 1.5f);
		}

		Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BoomerangProjectile>(), damage, knockback, player.whoAmI, player.altFunctionUse / 2, Type);
		return false;
	}
}