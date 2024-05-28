namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class Breastplate : Chestplate
{
	public override void PostRoll()
	{
		Item.defense = ItemLevel / 6 + 1;
	}
}