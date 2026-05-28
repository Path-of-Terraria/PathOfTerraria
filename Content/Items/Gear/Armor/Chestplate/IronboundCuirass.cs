namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class IronboundCuirass : DefenseChestplate
{
	protected override int MinimumDropItemLevel => 25;
	protected override int MaximumDropItemLevel => 50;
	protected override int MinimumDefense => 8;
	protected override int MaximumDefense => 15;
}
