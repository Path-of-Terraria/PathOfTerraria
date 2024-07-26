namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class Visor : Helmet
{
	public override void PostRoll(Item item)
	{
		Item.defense = ItemLevel / 10 + 1;
	}
}