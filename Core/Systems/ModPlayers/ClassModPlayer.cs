using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.ModPlayers;

public class ClassModPlayer : ModPlayer
{
	public string SelectedClass;
	
	/// <summary>
	/// Overrides the vanilla starting inventory to clear it
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
		var item = new Item();
		item.SetDefaults(ItemID.CopperPickaxe);
		vanillaItems.Add(item);

		item = new Item();
		item.SetDefaults(ItemID.CopperAxe);
		vanillaItems.Add(item);
	}

	public override void Initialize()
	{
		SelectedClass = "";
	}

	public override void SaveData(TagCompound tag)
	{
		tag["selectedClass"] = SelectedClass;
	}

	public override void LoadData(TagCompound tag)
	{
		SelectedClass = tag.GetString("selectedClass");
	}

	public bool HasSelectedClass()
	{
		return !string.IsNullOrEmpty(SelectedClass);
	}
}