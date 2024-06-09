using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.ModPlayers;

public class ClassModPlayer : ModPlayer
{
	public PlayerClass SelectedClass;
	
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
		SelectedClass = PlayerClass.None;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["selectedClass"] = (int) SelectedClass;
	}

	public override void LoadData(TagCompound tag)
	{
		int selectedClassInt = tag.GetInt("selectedClass");
		SelectedClass = (PlayerClass) selectedClassInt;
	} 

	public bool HasSelectedClass()
	{
		return SelectedClass != PlayerClass.None;
	}
}