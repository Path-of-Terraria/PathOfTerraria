using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.NPCs.ConditionalDropping;

internal class ConditionalDropIO : ModSystem
{
	public override void SaveWorldData(TagCompound tag)
	{
		Dictionary<int, int> countsByID = ConditionalDropHandler.PlayerCountByItemIds;

		if (countsByID.Count > 0)
		{
			tag.Add("count", countsByID.Count);
			int count = 0;

			foreach (KeyValuePair<int, int> pair in countsByID)
			{
				TagCompound pairTag = [];
				pairTag.Add("id", pair.Key);
				pairTag.Add("count", pair.Value);
				tag.Add("pair" + count++, pairTag);
			}
		}
	}

	public override void LoadWorldData(TagCompound tag)
	{
		ConditionalDropHandler.PlayerCountByItemIds.Clear();

		if (tag.TryGet("count", out int count))
		{
			for (int i = 0; i < count; i++)
			{
				TagCompound pairTag = tag.GetCompound("pair" + i);
				ConditionalDropHandler.PlayerCountByItemIds.Add(pairTag.GetInt("id"), pairTag.GetInt("count"));
			}
		}
	}
}
