namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class BatteredHelm : DefenseHelmet
{
	protected override int MinimumDropItemLevel => 1;
	protected override int MaximumDropItemLevel => 35;
	protected override int MinimumDefense => 1;
	protected override int MaximumDefense => 3;
}
