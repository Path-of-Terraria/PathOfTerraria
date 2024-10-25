using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Projectiles.Magic;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal abstract class Staff : Gear
{
	protected abstract int StaffType { get; }

	protected override string GearLocalizationCategory => "Staff";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetText("Mods.PathOfTerraria.Gear.Staff.AltUse");

		Item.staff[Type] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.DefaultToStaff(ModContent.ProjectileType<SparklingBallProjectile>(), 16, 25, 12);
		Item.UseSound = SoundID.Item20;
		Item.SetWeaponValues(20, 1);
		Item.SetShopValues(ItemRarityColor.Green2, 150);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.noUseGraphic = true;
		Item.channel = true;
		Item.autoReuse = false;
		Item.mana = 12;
	}

	public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
	{
		mult = 0;
	}

	public override bool AltFunctionUse(Player player)
	{
		return !player.GetModPlayer<AltUsePlayer>().OnCooldown;
	}

	public override bool CanUseItem(Player player)
	{
		if (player.altFunctionUse == 2)
		{
			const int ActiveTime = 120;

			player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = ActiveTime;
			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(60 * 10, ActiveTime);
			return false;
		}

		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, player.Center, Vector2.Zero, StaffType, 0, 0, player.whoAmI);

		return true;
	}
}