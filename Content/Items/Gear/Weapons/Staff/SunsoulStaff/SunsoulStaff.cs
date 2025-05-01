using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff.SunsoulStaff;

internal class SunsoulStaffItem : Staff
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 17;
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<SunsoulHead>();
		Item.damage = 120;
		Item.knockBack = 1;
	}

	public override bool CanUseItem(Player player)
	{
		return player.ownedProjectileCounts[ModContent.ProjectileType<StaffHeldProjectile>()] == 0;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse == 2)
		{
			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(AltCooldownTime, AltActiveTime);

			Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<SunsoulSummon>(), 0, 0, player.whoAmI);
			return false;
		}

		Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<StaffHeldProjectile>(), 0, 0, player.whoAmI, Item.type);
		return true;
	}
}
