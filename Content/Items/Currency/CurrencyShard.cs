using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// Base class for currency shards. Defaults to having a 4-frame animation, consumable, and has a default <see cref="CanRightClick"/>.
/// </summary>
public abstract class CurrencyShard : ModItem, GenerateNameAffixes.IItem
{
	protected virtual int FrameCount => 4;
	public virtual bool SupportsMouseItemTargeting => true;

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

internal sealed class CurrencyShardMouseItemTargetingSystem : ModSystem
{
	public override void Load()
	{
		On_ItemSlot.RightClick_ItemArray_int_int += ApplyMouseHeldShardToClickedItem;
	}

	private static void ApplyMouseHeldShardToClickedItem(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
	{
		bool clicked = Main.mouseRight && Main.mouseRightRelease;

		if (!clicked
			|| context != ItemSlot.Context.InventoryItem
			|| slot < 0
			|| slot >= inv.Length
			|| inv[slot].IsAir
			|| Main.mouseItem.ModItem is not CurrencyShard shard
			|| !shard.SupportsMouseItemTargeting)
		{
			orig(inv, context, slot);
			return;
		}

		Item targetItem = inv[slot];

		if (!shard.CanUseInPouch(targetItem, out _))
		{
			return;
		}

		shard.ApplyToItem(targetItem);
		Main.mouseItem.stack--;

		if (Main.mouseItem.stack <= 0)
		{
			Main.mouseItem.TurnToAir();
		}

		SoundEngine.PlaySound(SoundID.Grab);
		Main.mouseRightRelease = false;
	}
}
