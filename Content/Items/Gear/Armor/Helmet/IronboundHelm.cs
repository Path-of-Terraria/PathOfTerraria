namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class IronboundHelm : DefenseHelmet
{
	protected override int MinimumDropItemLevel => 25;
	protected override int MaximumDropItemLevel => 50;
	protected override int MinimumDefense => 5;
	protected override int MaximumDefense => 8;
}
