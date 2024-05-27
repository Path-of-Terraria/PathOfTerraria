using PathOfTerraria.Core.Systems;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Melee;

internal class Sword : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapon/Sword/Base";

	public override float DropChance => 1f;

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
		GearType = GearType.Sword;
	}

	public override bool AltFunctionUse(Player player)
	{
		AltUseSystem modPlayer = player.GetModPlayer<AltUseSystem>();

		// If cooldown is still active, do not allow alt usage.
		if (modPlayer.AltFunctionCooldown > 0 || !modPlayer.Player.CheckMana(5))
			return false;

		// Otherwise, set the cooldown and allow alt usage.
		modPlayer.AltFunctionCooldown = 180;
		return true;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
		Vector2 velocity, int type, int damage, float knockback)
	{
		if (player.altFunctionUse != 2) 
			return false;
			
		AltUseSystem modPlayer = player.GetModPlayer<AltUseSystem>();
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

	public override string GenerateName()
	{
		string prefix = Main.rand.Next(5) switch
		{
			0 => "Sharp",
			1 => "Harmonic",
			2 => "Enchanted",
			3 => "Shiny",
			4 => "Strange",
			_ => "Unknown"
		};

		string suffix = Main.rand.Next(5) switch
		{
			0 => "Drape",
			1 => "Dome",
			2 => "Thought",
			3 => "Vision",
			4 => "Maw",
			_ => "Unknown"
		};

		string item = GetType().ToString();
		return Rarity switch
		{
			GearRarity.Normal => item,
			GearRarity.Magic => $"{prefix} {item}",
			GearRarity.Rare => $"{prefix} {suffix} {item}",
			GearRarity.Unique => Item.Name,
			_ => "Unknown Item"
		};
	}
}