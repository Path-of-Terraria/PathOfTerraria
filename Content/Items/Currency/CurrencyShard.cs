using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// Base class for currency shards. As of now, does nothing.
/// </summary>
internal abstract class CurrencyShard : ModItem
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

		PoTInstanceItemData item = Main.LocalPlayer.HeldItem.GetInstanceData();
		return !item.Corrupted && !item.Cloned;
	}
}