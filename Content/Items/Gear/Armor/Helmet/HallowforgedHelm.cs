namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class HallowforgedHelm : DefenseHelmet
{
	protected override int MinimumDropItemLevel => 40;
	protected override int MaximumDropItemLevel => 65;
	protected override int MinimumDefense => 10;
	protected override int MaximumDefense => 13;
}
