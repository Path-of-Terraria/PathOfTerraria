using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// Base class for currency shards. Defaults to having a 4-frame animation, consumable, and has a default <see cref="CanRightClick"/>.
/// </summary>
public abstract class CurrencyShard : ModItem, GenerateNameAffixes.IItem
{
	protected virtual int FrameCount => 4;

	public override void SetStaticDefaults()
	{
		ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, FrameCount));
		
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
		return CanUseInPouch(Main.LocalPlayer.HeldItem, out _);
	}

	public override void RightClick(Player player)
	{
		ApplyToItem(player.HeldItem);
	}

	protected static bool DefaultValidityCheck(Item item, out string failKey)
	{
		if (!item.TryGetGlobalItem(out PoTGlobalItem _))
		{
			failKey = "Invalid";
			return false;
		}

		failKey = null;
		PoTInstanceItemData data = item.GetInstanceData();

		if (data.Corrupted || data.Cloned)
		{
			failKey = "CorruptOrCloned";
			return false;
		}

		return true;
	}

	(sbyte, sbyte) GenerateNameAffixes.IItem.GenerateAffixIds()
	{
		return (-1, -1);
	}

	public abstract bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey);

	public abstract void ApplyToItem(Item slotItem);
}