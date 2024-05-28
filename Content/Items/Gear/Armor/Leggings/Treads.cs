namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Treads : Leggings
{
	public override void PostRoll()
	{
		Item.defense = ItemLevel / 14 + 1;
	}
}