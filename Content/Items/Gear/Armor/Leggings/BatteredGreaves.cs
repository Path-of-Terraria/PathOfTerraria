namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class BatteredGreaves : DefenseLeggings
{
	protected override int MinimumDropItemLevel => 1;
	protected override int MaximumDropItemLevel => 35;
	protected override int MinimumDefense => 1;
	protected override int MaximumDefense => 3;
}
