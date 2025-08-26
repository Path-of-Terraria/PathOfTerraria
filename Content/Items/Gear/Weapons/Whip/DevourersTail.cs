using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Content.Projectiles.Whip;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class DevourersTail : Whip
{
	public override WhipDrawData DrawData => new(new(14, 14), new(0, 14, 14, 26), new(0, 34, 14, 24), new(0, 40, 14, 24), new(0, 64, 14, 28), false);
	
	public override WhipSettings WhipSettings => new()
	{
		Segments = 33,
		RangeMultiplier = 3f,
	};

	public override Action<Projectile> UpdateProjectile => proj =>
	{
		Player plr = Main.player[proj.owner];

		if (plr.GetModPlayer<AltUsePlayer>().AltFunctionActive)
		{
			plr.channel = false;
		}
	};

	public override ProjectileModifyLinkDrawDelegate ModifyProjectileLinkDrawing => (Projectile proj, int segment, ref Color color) =>
	{
		Player plr = Main.player[proj.owner];

		if (!plr.GetModPlayer<AltUsePlayer>().AltFunctionActive)
		{
			return;
		}

		color = Color.Lerp(color, Color.White, segment / 33f);
	};
	
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

		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 67, 2, 4);
		Item.channel = true;
		Item.value = Item.buyPrice(0, 5, 0, 0);
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		velocity = player.DirectionTo(Main.MouseWorld) * 4;

		if (player.GetModPlayer<AltUsePlayer>().AltFunctionActive)
		{
			damage = (int)(damage * 1.5f);
		}
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse == 2)
		{
			player.itemAnimation = player.itemAnimationMax = 24;
			Item.useAnimation = 24;

			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(60 * 15, 60 * 8);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModContent.GetInstance<SyncAltUseHandler>().Send((byte)player.whoAmI, 60 * 15, 60 * 8);
			}

			return true;
		}
		else if (!player.GetModPlayer<AltUsePlayer>().AltFunctionActive)
		{
			player.itemAnimation = player.itemAnimationMax = 30;
			Item.useAnimation = 30;
		}

		return true;
	}
}