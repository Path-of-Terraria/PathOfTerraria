using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Talismans;

internal class YellowCore : Talisman<AddedSummonCritAffix>
{
	protected override float AffixStrength => 11f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(22, 24);
		Item.value = Item.buyPrice(0, 0, 1, 0);
		Item.defense = 1;
	}
}