using PathOfTerraria.Common.Systems.Networking.Handlers;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.NPCs.ConditionalDropping;

/// <summary>
/// Controls 'conditional' drops, per player. This keeps data from bleeding or being inconsistent.
/// </summary>
internal class ConditionalDropPlayer : ModPlayer
{
	internal readonly HashSet<int> TrackedIds = [];

	public void AddId(int id, bool syncing = false)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient && !syncing)
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

	public void RemoveId(int id, bool syncing = false)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient && !syncing)
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
			tag.Add("ids", (string[])[.. TrackedIds.Select(x => x < ItemID.Count ? "Terraria/" + x : ContentSamples.ItemsByType[x].ModItem.FullName)]);
		}
	}

	public override void OnEnterWorld()
	{
		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			ModContent.GetInstance<SyncNewConditionalDropPlayerHandler>().Send((byte)Player.whoAmI, (int[])[.. TrackedIds]);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		TrackedIds.Clear();

		if (!tag.ContainsKey("ids"))
		{
			return;
		}

		if (tag.TryGet("ids", out object[] obj))
		{
			if (obj.Length == 0)
			{
				return;
			}

			if (obj[0] is not string)
			{
				return;
			}
		}

		if (tag.TryGet("ids", out string[] ids))
		{
			foreach (string name in ids)
			{
				int id;

				if (name.StartsWith("Terraria/"))
				{
					id = int.Parse(name.Split('/')[1]);
				}
				else
				{
					if (ModContent.TryFind(name, out ModItem item))
					{
						id = item.Type;	
					}
					else
					{
						id = -1;
					}
				}

				if (id != -1)
				{
					TrackedIds.Add(id);
				}
			}
		}
	}
}