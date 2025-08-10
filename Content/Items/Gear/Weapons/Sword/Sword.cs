using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal abstract class Sword : Gear
{
	private const int AltCooldownTime = 60 * 4;
	
	protected override string GearLocalizationCategory => "Sword";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetText("Mods.PathOfTerraria.Gear.Sword.AltUse");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 10;
		Item.width = 40;
		Item.height = 40;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 25;
		Item.useAnimation = 25;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Melee;
		Item.knockBack = 6;
		Item.crit = 6;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.PurificationPowder;
		Item.useTurn = true;
		Item.shootSpeed = 10f;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}
	
	public override bool AltFunctionUse(Player player)
	{
		if (!player.CheckMana(5))
		{
			return false;
		}
		
		return !player.GetModPlayer<AltUsePlayer>().OnCooldown;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
		Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2 || !player.ItemAnimationJustStarted)
		{
			return false;
		}

		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();
		modPlayer.Player.statMana -= 5;
		Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(AltCooldownTime);

		return false;
	}

	public override void ModifyShootStats(
		Player player,
		ref Vector2 position,
		ref Vector2 velocity,
		ref int type,
		ref int damage,
		ref float knockback)
	{
		type = ModContent.ProjectileType<LifeStealProjectile>();
	}
}