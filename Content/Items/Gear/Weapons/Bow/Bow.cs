using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Systems;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal abstract class Bow : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Bow/{GetType().Name}";

	/// <summary>
	/// Stores a Bow's sprite asset automatically for use in <see cref="BowAnimationProjectile"/>.
	/// </summary>
	public static Dictionary<int, Asset<Texture2D>> BowProjectileSpritesById = [];

	public override float DropChance => 1f;
	public override int ItemLevel => 1;

	protected virtual int AnimationSpeed => 6;
	protected virtual float CooldownTimeInSeconds => 5;

	private bool _isChanneling;

	public override void SetStaticDefaults()
	{
		if (ModContent.HasAsset(Texture + "Animated"))
		{
			BowProjectileSpritesById.Add(Type, ModContent.Request<Texture2D>(Texture + "Animated"));
		}
	}

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
		return true;// player.GetModPlayer<AltUseSystem>().AltFunctionCooldown <= 0;
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