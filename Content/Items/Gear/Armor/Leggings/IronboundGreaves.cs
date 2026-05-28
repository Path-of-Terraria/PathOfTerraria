namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class IronboundGreaves : DefenseLeggings
{
	protected override int MinimumDropItemLevel => 25;
	protected override int MaximumDropItemLevel => 50;
	protected override int MinimumDefense => 3;
	protected override int MaximumDefense => 7;
}
