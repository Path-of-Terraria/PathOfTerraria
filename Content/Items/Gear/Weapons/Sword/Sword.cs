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

	/// <summary>
	/// Standard sword alternate use is input-driven so it can be used during a swing.
	/// Unique swords opt out by default and retain their own alternate-use behavior.
	/// </summary>
	protected virtual bool HasIndependentLifeStealAlt => !Item.GetStaticData().IsUnique;
	
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
		return false;
	}

	public override void HoldItem(Player player)
	{
		if (!HasIndependentLifeStealAlt || player.whoAmI != Main.myPlayer || !Main.mouseRight || !Main.mouseRightRelease)
		{
			return;
		}

		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();
		if (!altUsePlayer.AltFunctionAvailable || !player.CheckMana(5, false, false))
		{
			return;
		}

		player.CheckMana(5, true);

		Vector2 position = player.RotatedRelativePoint(player.MountedCenter);
		Vector2 velocity = player.DirectionTo(Main.MouseWorld) * Item.shootSpeed;
		int damage = (int)player.GetWeaponDamage(Item);
		float knockback = player.GetWeaponKnockback(Item, Item.knockBack);
		IEntitySource source = player.GetSource_ItemUse(Item);
		Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<LifeStealProjectile>(), damage, knockback, player.whoAmI);

		altUsePlayer.SetAltCooldown(AltCooldownTime);
		Main.mouseRightRelease = false;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
		Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2 || !player.ItemAnimationJustStarted)
		{
			return false;
		}

		player.CheckMana(5, true);
		Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
		player.GetModPlayer<AltUsePlayer>().SetAltCooldown(AltCooldownTime);

		return false;
	}

}
