using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;
namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that upgrades a Magic item to Rare, keeping its existing affixes and adding one new one.
/// </summary>
public class RegalShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 350f;
		staticData.MinDropItemLevel = 15;
	}
	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}
		if (slotItem.GetInstanceData().Rarity != ItemRarity.Magic)
		{
			failKey = "NotMagic";
			return false;
		}
		return true;
	}
	public override void RightClick(Player player)
	{
		base.RightClick(player);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}
	public override void ApplyToItem(Item slotItem)
	{
		PoTInstanceItemData data = slotItem.GetInstanceData();
		data.Rarity = ItemRarity.Rare;
		PoTItemHelper.AddNewAffix(slotItem, data);
		data.NameAffix = GenerateNameAffixes.Invoke(slotItem);
	}
}