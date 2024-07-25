using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Common.Systems;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal abstract class Boomerang : Gear
{
	public override float DropChance => 1f;
	public override int ItemLevel => 1;
	public override string AltUseDescription => Language.GetTextValue("Mods.PathOfTerraria.Gear.Boomerang.AltUse");

	protected virtual int BoomerangCount => 1;
	protected override string GearLocalizationCategory => "Boomerang";
	
	public override void Defaults()
	{
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