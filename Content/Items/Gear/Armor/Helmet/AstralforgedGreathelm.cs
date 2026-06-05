namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class AstralforgedGreathelm : DefenseHelmet
{
	protected override int MinimumDropItemLevel => 55;
	protected override int MaximumDropItemLevel => 80;
	protected override int MinimumDefense => 15;
	protected override int MaximumDefense => 20;
}
