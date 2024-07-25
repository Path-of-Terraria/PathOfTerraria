using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal abstract class Battleaxe : Gear
{
	public override float DropChance => 1f;
	public override string AltUseDescription => Language.GetTextValue("Mods.PathOfTerraria.Gear.Battleaxe.AltUse");
	protected override string GearLocalizationCategory => "Battleaxe";

	public override void Defaults()
	{
		Item.damage = 10;
		Item.width = 48;
		Item.height = 48;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 40;
		Item.useAnimation = 40;
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
}