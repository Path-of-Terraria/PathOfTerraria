using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal abstract class Bow : Gear
{

	/// <summary>
	/// Stores a Bow's sprite asset automatically for use in <see cref="BowAnimationProjectile"/>.
	/// </summary>
	public static Dictionary<int, Asset<Texture2D>> BowProjectileSpritesById = [];

	public int ItemLevel
	{
		get => 1;
		set => this.GetInstanceData().RealLevel = value; // Technically preserves previous behavior.
	}

	protected override string GearLocalizationCategory => "Bow";
	protected virtual int AnimationSpeed => 6;
	protected virtual float CooldownTimeInSeconds => 5;

	protected bool IsChanneling;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetText("Mods.PathOfTerraria.Gear.Bow.AltUse");

		if (ModContent.HasAsset(Texture + "Animated"))
		{
			BowProjectileSpritesById.Add(Type, ModContent.Request<Texture2D>(Texture + "Animated"));
		}
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.CloneDefaults(ItemID.WoodenBow);
		Item.width = 32;
		Item.height = 64;
		Item.useTime = 60;
		Item.useAnimation = 60;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.channel = true;
		Item.UseSound = null;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Bow;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);

		DoHoldFunctionality(player);
	}

	protected void DoHoldFunctionality(Player player)
	{
		if (player.altFunctionUse != 2 || !player.channel)
		{
			IsChanneling = false;
			Item.noUseGraphic = false;
			return;
		}

		//Check if we already started the animation
		if (IsChanneling)
		{
			return;
		}

		//We are starting to channel - Begin the animation
		Item.noUseGraphic = true;
		IsChanneling = true;

		if (Main.myPlayer != player.whoAmI)
		{
			return;
		}

		Vector2 vel = player.DirectionTo(Main.MouseWorld) * Item.shootSpeed;
		int type = ModContent.ProjectileType<BowAnimationProjectile>();
		Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, vel, type, Item.damage, Item.knockBack, player.whoAmI, AnimationSpeed, CooldownTimeInSeconds);
	}

	public override bool AltFunctionUse(Player player)
	{
		return player.GetModPlayer<AltUsePlayer>().AltFunctionCooldown <= 0;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2)
		{
			SoundEngine.PlaySound(SoundID.Item5, player.Center); // Play sound here to not make it play twice when alt firing
		}

		return player.altFunctionUse != 2;
	}
}