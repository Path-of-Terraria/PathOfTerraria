namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class HallowforgedCuirass : DefenseChestplate
{
	protected override int MinimumDropItemLevel => 40;
	protected override int MaximumDropItemLevel => 65;
	protected override int MinimumDefense => 16;
	protected override int MaximumDefense => 23;
}
