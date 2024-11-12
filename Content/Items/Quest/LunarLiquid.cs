using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class LunarLiquid : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.rare = ItemRarityID.Quest;
		Item.Size = new Vector2(24, 18);
		Item.consumable = false;
		Item.buffTime = 25;
		Item.buffType = BuffID.VortexDebuff;
		Item.useTime = Item.useAnimation = 5;
		Item.useStyle = ItemUseStyleID.DrinkLiquid;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		tooltips.RemoveAll(x => x.Name == "BuffTime");
	}
}
