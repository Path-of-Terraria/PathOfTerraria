namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class EternalforgedAegis : DefenseChestplate
{
	protected override int MinimumDropItemLevel => 71;
	protected override int MaximumDropItemLevel => 85;
	protected override int MinimumDefense => 35;
	protected override int MaximumDefense => 43;
}
