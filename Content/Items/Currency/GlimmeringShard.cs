using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
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
		if (!Main.LocalPlayer.HeldItem.TryGetGlobalItem(out PoTGlobalItem _))
		{
			return false;
		}

		PoTInstanceItemData data = Main.LocalPlayer.HeldItem.GetInstanceData();
		return data.Rarity == ItemRarity.Magic && !data.Corrupted;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);
	}
}
