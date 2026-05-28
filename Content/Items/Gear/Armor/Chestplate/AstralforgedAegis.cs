namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class AstralforgedAegis : DefenseChestplate
{
	protected override int MinimumDropItemLevel => 55;
	protected override int MaximumDropItemLevel => 80;
	protected override int MinimumDefense => 25;
	protected override int MaximumDefense => 33;
}
