using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Content.Items.Quest;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.NPCs.ConditionalDropping;

/// <summary>
/// Controls 'conditional' drops, per player. This keeps data from bleeding or being inconsistent.
/// </summary>
internal class ConditionalDropPlayer : ModPlayer
{
	internal readonly HashSet<int> TrackedIds = [];

	public void AddId(int id)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModContent.GetInstance<SyncConditionalDropHandler>().Send((byte)Player.whoAmI, id, true);
			return;
		}

		TrackedIds.Add(id);
	}

	public void AddId<T>() where T : ModItem
	{
		AddId(ModContent.ItemType<T>());
	}

	public void RemoveId(int id)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModContent.GetInstance<SyncConditionalDropHandler>().Send((byte)Player.whoAmI, id, false);
			return;
		}

		TrackedIds.Remove(id);
	}

	public void RemoveId<T>() where T : ModItem
	{
		RemoveId(ModContent.ItemType<T>());
	}

	public override void SaveData(TagCompound tag)
	{
		if (TrackedIds.Count > 0)
		{
			tag.Add("ids", (int[])[.. TrackedIds]);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		TrackedIds.Clear();

		if (tag.TryGet("ids", out int[] ids))
		{
			foreach (int id in ids)
			{
				TrackedIds.Add(id);
			}
		}
	}
}