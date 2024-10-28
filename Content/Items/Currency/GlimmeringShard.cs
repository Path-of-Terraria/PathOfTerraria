using PathOfTerraria.Core.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

internal class GlimmeringShard : CurrencyShard
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(30, 28);
		Item.rare = ItemRarityID.Green;
		Item.consumable = true; // Purely for the tooltip line
	}

	public override bool CanRightClick()
	{
		return Main.LocalPlayer.HeldItem.TryGetGlobalItem(out PoTGlobalItem _) && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity == Common.Enums.ItemRarity.Magic;
	}

	public override void RightClick(Player player)
	{
		if (player.HeldItem.TryGetGlobalItem(out PoTGlobalItem _) && player.HeldItem.GetInstanceData().Rarity == Common.Enums.ItemRarity.Magic)
		{
			// Clear and reroll affixes
			player.HeldItem.GetInstanceData().Affixes.Clear();
			PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);
		}
		else
		{
			Item.stack++;
		}
	}
}
