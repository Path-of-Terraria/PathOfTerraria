﻿using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Systems;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal abstract class Bow : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Bow/WoodenBow";

	public override float DropChance => 1f;
	public override int ItemLevel => 1;
	private bool _isChanneling;
	
	public override void Defaults()
	{
		Item.CloneDefaults(ItemID.WoodenBow);
		Item.width = 32;
		Item.height = 64;
		Item.useTime = 60;
		Item.useAnimation = 60;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.channel = true;
		Item.UseSound = null;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);
		if (player.altFunctionUse != 2 || !player.channel)
		{
			_isChanneling = false;
			Item.noUseGraphic = false;
			return;
		}

		//Check if we already started the animation
		if (_isChanneling)
		{
			return;
		}

		//We are starting to channel - Begin the animation
		Item.noUseGraphic = true;
		_isChanneling = true;
		Projectile.NewProjectileDirect(
			player.GetSource_ItemUse(Item),
			player.Center,
			player.DirectionTo(Main.MouseWorld) * Item.shootSpeed,
			ModContent.ProjectileType<WoodenBowAnimationProjectile>(),
			Item.damage,
			Item.knockBack,
			player.whoAmI);
	}

	public override bool AltFunctionUse(Player player)
	{
		return player.GetModPlayer<AltUseSystem>().AltFunctionCooldown <= 0;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2)
		{
			SoundEngine.PlaySound(SoundID.Item5, player.Center);
		}

		return player.altFunctionUse != 2;
	}
	
	public override string GeneratePrefix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Swift",
			1 => "Piercing",
			2 => "Hunter's",
			3 => "Gale",
			4 => "Vengeful",
			_ => "Unknown"
		};
	}
	
	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Flight",
			1 => "Song",
			2 => "Wind",
			3 => "Thorn",
			4 => "Sight",
			_ => "Unknown"
		};
	}
}