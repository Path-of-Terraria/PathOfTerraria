using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

internal abstract class Staff : Gear
{
	public const int AltActiveTime = 60 * 4;
	public const int AltCooldownTime = 60 * 14;

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

		// Base shoot is set to BeeArrow so it's obvious if the user misses setting the shot projectile ID
		Item.DefaultToStaff(ProjectileID.BeeArrow, 16, 25, 12);
		Item.UseSound = SoundID.Item20;
		Item.SetWeaponValues(20, 1);
		Item.SetShopValues(ItemRarityColor.Green2, 150);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.noUseGraphic = true;
		Item.channel = true;
		Item.autoReuse = true;
		Item.mana = 12;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Staff;
	}

	public override bool? CanAutoReuseItem(Player player)
	{
		return player.GetModPlayer<StaffPlayer>().Empowered;
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
			player.GetModPlayer<StaffPlayer>().EmpoweredStaffTime = AltActiveTime;
			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(AltCooldownTime, AltActiveTime);

			if (player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
			{
				SyncStaffAltHandler.Send((byte)player.whoAmI);
			}

			return false;
		}

		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<StaffHeldProjectile>(), 0, 0, player.whoAmI, Item.type);

		return true;
	}
}