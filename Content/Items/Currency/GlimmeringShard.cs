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
		Item.consumable = true; // Purely for the tooltip line
	}

	public override bool CanRightClick()
	{
		return true;
	}

	public override void RightClick(Player player)
	{
		if (player.HeldItem.TryGetGlobalItem(out PoTGlobalItem _) && player.HeldItem.GetInstanceData().Rarity == Common.Enums.ItemRarity.Magic)
		{
			player.HeldItem.GetInstanceData().Affixes.Clear();
			PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);

			foreach (var s in player.HeldItem.GetInstanceData().Affixes)
			Main.NewText(s.Name);
		}
		else
		{
			Item.stack++;
		}
	}
}
