using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that will reroll the values of the affixes of an item
/// </summary>
internal class RadiantShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 100f;
		staticData.MinDropItemLevel = 25;
	}

	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity != ItemRarity.Normal;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.RerollAffixValues(player.HeldItem);
	}
}
