namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class BatteredMail : DefenseChestplate
{
	protected override int MinimumDropItemLevel => 1;
	protected override int MaximumDropItemLevel => 35;
	protected override int MinimumDefense => 2;
	protected override int MaximumDefense => 7;
}
