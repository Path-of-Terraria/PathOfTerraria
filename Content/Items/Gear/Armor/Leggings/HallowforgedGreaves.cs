namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class HallowforgedGreaves : DefenseLeggings
{
	protected override int MinimumDropItemLevel => 40;
	protected override int MaximumDropItemLevel => 65;
	protected override int MinimumDefense => 8;
	protected override int MaximumDefense => 11;
}
