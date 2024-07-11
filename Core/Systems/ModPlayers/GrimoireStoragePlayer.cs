using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.ModPlayers;

internal class GrimoireStoragePlayer : ModPlayer
{
	public readonly List<Item> Storage = [];

	public override void SaveData(TagCompound tag)
	{
		tag.Add("count", Storage.Count);

		for (int i = 0; i < Storage.Count; i++)
		{
			Item item = Storage[i];
			tag.Add("item" + i, ItemIO.Save(item));
		}
	}

	public override void LoadData(TagCompound tag)
	{
		int count = tag.GetInt("count");

		for (int i = 0; i < count; i++)
		{
			Storage.Add(ItemIO.Load(tag.GetCompound("item" + i)));
		}	
	}
}
