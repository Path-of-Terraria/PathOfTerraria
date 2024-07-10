using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class GrimoireItem : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Grimoire/{GetType().Name}";
	public override float DropChance => 1f;
	public override string AltUseDescription => Language.GetTextValue("Mods.PathOfTerraria.Gear.Grimoire.AltUse");
	protected override string GearLocalizationCategory => "Grimoire";

	public override void Defaults()
	{
		Item.damage = 10;
		Item.width = 30;
		Item.height = 34;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useTime = 40;
		Item.useAnimation = 40;
		Item.autoReuse = true;
		Item.DamageType = DamageClass.Summon;
		Item.knockBack = 8;
		Item.crit = 12;
		Item.UseSound = SoundID.Item1;

		ItemType = ItemType.Magic;
	}
}