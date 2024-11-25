using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class StartingItemsModPlayer : ModPlayer
{
	/// <summary>
	/// Clears the vanilla inventory
	/// </summary>
	/// <param name="itemsByMod"></param>
	/// <param name="mediumCoreDeath"></param>
	public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
	{
		if (!itemsByMod.TryGetValue("Terraria", out List<Item> vanillaItems))
		{
			return;
		}

		vanillaItems.Clear();
	}
	
	/// <summary>
	/// Adds the starting items for the mod
	/// </summary>
	/// <param name="mediumCoreDeath"></param>
	/// <returns></returns>
	public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
		var woodenSword = new Item(ItemID.WoodenSword);
		PoTInstanceItemData data = woodenSword.GetInstanceData();
		data.Rarity = ItemRarity.Magic;
		PoTItemHelper.Roll(woodenSword, PoTItemHelper.PickItemLevel());
		
		var woodenBow = new Item(ItemID.WoodenBow);
		data = woodenBow.GetInstanceData();
		data.Rarity = ItemRarity.Magic;
		PoTItemHelper.Roll(woodenBow, PoTItemHelper.PickItemLevel());
		
		return [
			woodenSword,
			new Item(ItemID.CopperPickaxe),
			new Item(ItemID.CopperAxe),
			woodenBow,
		];
	}
}