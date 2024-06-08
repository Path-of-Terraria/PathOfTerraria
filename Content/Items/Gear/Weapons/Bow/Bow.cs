using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Systems;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal abstract class Bow : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Bow/WoodenBow";

	public override float DropChance => 1f;
	public override int ItemLevel => 1;
	
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.WoodenBow);
		Item.width = 16;
		Item.height = 64;
		Item.useTime = 60;
		Item.useAnimation = 50;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.channel = true;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);
		if (player.altFunctionUse != 2 || !player.channel)
		{
			return;
		}

		// Perform channeling action while right-click is held down
		if (!player.channel)
		{
			return;
		}

		//We are channeling - Begin the animation
		//TODO - Hide the normal held item until channeling is finished
		Projectile.NewProjectileDirect(
			player.GetSource_ItemUse(Item), 
			player.Center,
			player.DirectionTo(Main.MouseWorld) * Item.shootSpeed,
			ModContent.ProjectileType<BowDrawAnimationProjectile>(),
			Item.damage,
			Item.knockBack,
			player.whoAmI);
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
		Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2)
		{
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
		
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
		if (player.altFunctionUse != 2)
		{
			return;
		}
		
		type = ModContent.ProjectileType<BowDrawAnimationProjectile>();
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