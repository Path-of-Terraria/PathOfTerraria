using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal abstract class Battleaxe : Gear
{
	public override float DropChance => 1f;

	public override void Defaults()
	{
		Item.damage = 10;
		Item.width = 48;
		Item.height = 48;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 25;
		Item.useAnimation = 25;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Melee;
		Item.knockBack = 8;
		Item.crit = 12;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.PurificationPowder;
		Item.shootSpeed = 10f;
		ItemType = ItemType.Sword;
	}

	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		if (modPlayer.AltFunctionCooldown > 0 || player.statLife <= 5)
		{
			return false;
		}

		modPlayer.SetAltCooldown(180);
		player.statLife -= 5;
		return true;
	}

	public override string GeneratePrefix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Sharp",
			1 => "Harmonic",
			2 => "Enchanted",
			3 => "Shiny",
			4 => "Strange",
			_ => "Unknown"
		};
	}

	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Drape",
			1 => "Dome",
			2 => "Thought",
			3 => "Vision",
			4 => "Maw",
			_ => "Unknown"
		};
	}
}