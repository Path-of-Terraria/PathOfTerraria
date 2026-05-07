namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class AstralforgedSabatons : DefenseLeggings
{
	protected override int MinimumDropItemLevel => 55;
	protected override int MaximumDropItemLevel => 80;
	protected override int MinimumDefense => 12;
	protected override int MaximumDefense => 16;
}
