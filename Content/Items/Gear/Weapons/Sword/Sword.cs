using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Systems;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Sword : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/Base";

	protected override string GearLocalizationCategory => "Sword";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetTextValue("Mods.PathOfTerraria.Gear.Sword.AltUse");

	}

	public override void SetDefaults()
	{
		Item.damage = 10;
		Item.width = 40;
		Item.height = 40;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Melee;
		Item.knockBack = 6;
		Item.crit = 6;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.PurificationPowder;
		Item.shootSpeed = 10f;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}

	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		// If cooldown is still active, do not allow alt usage.
		if (!modPlayer.AltFunctionAvailable || !modPlayer.Player.CheckMana(5))
		{
			return false;
		}

		// Otherwise, set the cooldown and allow alt usage.
		modPlayer.SetAltCooldown(180);
		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
		Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2)
		{
			return false;
		}

		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();
		modPlayer.Player.statMana -= 5;
		Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

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
		type = ModContent.ProjectileType<Projectiles.LifeStealProjectile>();
	}
}