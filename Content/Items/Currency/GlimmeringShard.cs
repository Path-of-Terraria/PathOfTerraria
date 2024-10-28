using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

internal class GlimmeringShard : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(30, 28);
		Item.rare = ItemRarityID.Green;
	}

	private class GlimmeringShardRightClickCheck : GlobalItem
	{
		public override void RightClick(Item item, Player player)
		{
			item.stack++;

			if (player.HeldItem.type == ModContent.ItemType<GlimmeringShard>() && item.TryGetGlobalItem(out PoTGlobalItem potGlobal) 
				&& item.GetInstanceData().Rarity == Common.Enums.ItemRarity.Magic)
			{
				item.GetInstanceData().Affixes.Clear();
				PoTItemHelper.Roll(item, item.GetInstanceData().RealLevel);
			}
		}
	}
}
