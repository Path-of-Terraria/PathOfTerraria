using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CheaperShopsPassive : Passive
{
	public override void OnLoad()
	{
		On_Player.GetItemExpectedPrice += ModifyItemSellPrice;
	}

	private void ModifyItemSellPrice(On_Player.orig_GetItemExpectedPrice orig, Player self, Item item, out long calcForSelling, out long calcForBuying)
	{
		orig(self, item, out calcForSelling, out calcForBuying);

		if (self.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ValuableSalesPassive>(out float value))
		{
			calcForBuying = (long)(calcForSelling * (1 - value / 100f));
		}
	}
}
