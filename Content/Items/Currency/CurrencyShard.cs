using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// Base class for currency shards. Defaults to having a 4-frame animation, consumable, and has a default <see cref="CanRightClick"/>.
/// </summary>
public abstract class CurrencyShard : ModItem
{
	public override void SetStaticDefaults()
	{
		ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
		
		SetStaticData();
	}

	protected virtual void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
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