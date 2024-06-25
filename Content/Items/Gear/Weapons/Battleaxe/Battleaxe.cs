using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal abstract class Battleaxe : Gear
{
	public override float DropChance => 1f;
	public override string AltUseDescription => "Sacrifice 5 life to increase damage temporarily. Take more damage during the effect.";

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
		ItemType = ItemType.Sword;
	}

	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		if (modPlayer.AltFunctionCooldown > 0 || player.statLife <= 5)
		{
			return false;
		}

		modPlayer.SetAltCooldown(900);
		player.statLife -= 5;
		player.AddBuff(ModContent.BuffType<BattleaxeBuff>(), 300);
		return true;
	}

	public override string GeneratePrefix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Brutal",
			1 => "Savage",
			2 => "Cleaving",
			3 => "War",
			4 => "Bloodthirsty",
			_ => "Unknown"
		};
	}

	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Fury",
			1 => "Cleaver",
			2 => "Ravage",
			3 => "Hew",
			4 => "Slayer",
			_ => "Unknown"
		};
	}
}