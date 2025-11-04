using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Talismans;

internal class RichMahoganyTalisman : Talisman<IncreasedSummonDamageAffix>
{
	protected override float AffixStrength => 5f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(18, 36);
		Item.value = Item.buyPrice(0, 0, 1, 50);
		Item.defense = 2;
	}
}