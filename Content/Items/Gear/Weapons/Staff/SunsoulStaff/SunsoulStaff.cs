using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff.SunsoulStaff;

internal class SunsoulStaffItem : Staff
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.shoot = ModContent.ProjectileType<SunsoulHead>();
		Item.damage = 85;
		Item.knockBack = 1;
		Item.value = Item.buyPrice(0, 5, 0, 0);
	}

	public override void HoldItem(Player player)
	{
		if (Main.myPlayer == player.whoAmI && !player.GetModPlayer<AltUsePlayer>().OnCooldown && Main.mouseRight && !player.mouseInterface)
		{
			int damage = (int)(Item.damage * 1.5f);
			int type = ModContent.ProjectileType<SunsoulSummon>();
			int proj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, type, damage, 0, player.whoAmI);

			for (int i = 0; i < 20; ++i)
			{
				Dust.NewDustPerfect(player.Center, DustID.Torch, Main.rand.NextVector2Circular(3, 3));
			}

			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(35 * 60, 15 * 60);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
				SyncAltUseHandler.Send((byte)player.whoAmI, 35 * 60, 15 * 60);
			}
		}
	}

	public override bool AltFunctionUse(Player player)
	{
		return false;
	}

	public override bool CanUseItem(Player player)
	{
		return player.ownedProjectileCounts[ModContent.ProjectileType<StaffHeldProjectile>()] == 0;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		// Force-channel because the held staff breaks without it for some reason
		// Also, using less apt itemless overload since the one that takes item doesn't work
		player.StartChanneling();
		Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<StaffHeldProjectile>(), 0, 0, player.whoAmI, Item.type);
		return true;
	}
}
