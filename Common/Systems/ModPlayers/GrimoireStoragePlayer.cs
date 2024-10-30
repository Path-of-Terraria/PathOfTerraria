using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class GrimoireStoragePlayer : ModPlayer
{
	public readonly List<Item> Storage = [];

	public override void SaveData(TagCompound tag)
	{
		Storage.RemoveAll(x => x.IsAir || x.type == ItemID.None || x.stack == 0);

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

	internal Dictionary<int, int> GetStoredCount()
	{
		List<Item> storage = Player.GetModPlayer<GrimoireStoragePlayer>().Storage;
		Dictionary<int, int> stacksById = [];

		foreach (Item item in storage)
		{
			if (!stacksById.TryAdd(item.type, item.stack))
			{
				stacksById[item.type] += item.stack;
			}
		}

		return stacksById;
	}
}
