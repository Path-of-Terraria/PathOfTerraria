using PathOfTerraria.Content.Projectiles.Ranged;
using PathOfTerraria.Core.Systems;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal abstract class Boomerang : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Boomerangs/{GetType().Name}";
	public override float DropChance => 1f;
	public override int ItemLevel => 1;

	protected virtual int BoomerangCount => 1;

	public override void Defaults()
	{
		Item.CloneDefaults(ItemID.WoodenBoomerang);
		Item.width = 16;
		Item.height = 28;
		Item.useTime = 10;
		Item.useAnimation = 10;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.channel = true;
	}

	public override bool CanUseItem(Player player)
	{
		return player.ownedProjectileCounts[ModContent.ProjectileType<BoomerangProjectile>()] < BoomerangCount;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BoomerangProjectile>(), damage, knockback, player.whoAmI, player.altFunctionUse / 2, Type);

		if (player.altFunctionUse == 2)
		{
			player.GetModPlayer<AltUsePlayer>().SetAltCooldown(180);
		}

		return false;
	}

	public override string GeneratePrefix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Spiraling",
			1 => "Quick",
			2 => "Razor",
			3 => "Enchanted",
			4 => "Storm",
			_ => "Unknown"
		};
	}

	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Return",
			1 => "Slice",
			2 => "Glide",
			3 => "Arc",
			4 => "Edge",
			_ => "Unknown"
		};
	}
}