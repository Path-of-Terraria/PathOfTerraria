using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to clone an item that is not unique.
/// </summary>
public class EchoingShard : CurrencyShard
{
	protected override int FrameCount => 5;

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Master;
	}
	
	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		if (slotItem.GetInstanceData().Rarity == ItemRarity.Unique)
		{
			failKey = "IsUnique";
			return false;
		}

		failKey = null;
		return true;
	}

	public override void ApplyToItem(Item slotItem)
	{
		CloneItem(slotItem, Main.LocalPlayer);
	}

	/// <summary>
	/// Clones the item in the player's hand, adding the Cloned tag on the item
	/// </summary>
	/// <param name="player"></param>
	private static void CloneItem(Item item, Player player)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		Item clonedItem = item.Clone();
		PoTInstanceItemData clonedData = clonedItem.GetInstanceData();
		clonedData.Rarity = data.Rarity;
		clonedData.Influence = data.Influence;
		clonedData.ImplicitCount = data.ImplicitCount;
		clonedData.RealLevel = data.RealLevel;
		clonedData.Affixes = [.. data.Affixes];
		clonedData.NameAffix = data.NameAffix;
		clonedData.Cloned = true;

		var source = new Terraria.DataStructures.EntitySource_Misc("EchoingShard");
		player.QuickSpawnItem(source, clonedItem, 1);
	}
}