using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class Crown : Helmet
{
	public override void PostRoll(Item item)
	{
		Item.defense = GetItemLevel.Invoke(item) / 10 + 1;
	}
}