using Mono.Cecil;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Experience;
using PathOfTerraria.Core.Systems.ModPlayers;
using Steamworks;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Core.Systems.MobSystem;

internal class NoMapSpawns : GlobalNPC // no spawns for now; might want to have in the future... idk
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.AnyActive(PathOfTerraria.Instance))
		{
			pool.Clear();
		}
	}
}
internal class MobMappingSystem : ModSystem
{
	private static readonly Dictionary<NPC, Vector2> _savedNpcs = []; // inactive npcs
	private static readonly HashSet<NPC> _npcs = []; // active npcs

	public static void OpenSubworld()
	{
		foreach (NPC npc in _npcs) { npc.active = false; } // saved npcs are non-existent,
														   // so just clear (garbage collector prayge)

		_savedNpcs.Clear();
		_npcs.Clear();
	}

	private void SpawnNpc(NPC npc)
	{
		int availableNPCSlot = GetAvailableNPCSlot(npc.type, 0);
		if (availableNPCSlot == -1)
		{
			return; // no slots
		}
		
		_savedNpcs.Remove(npc);

		npc.whoAmI = availableNPCSlot;
		Main.npc[npc.whoAmI] = npc;
		_npcs.Add(npc);
	}

	private void DespawnNpc(NPC npc)
	{
		Main.npc[npc.whoAmI] = new();

		_savedNpcs.Add(npc, npc.Center);
		_npcs.Remove(npc);
	}

	public override void PreUpdateNPCs()
	{
		if (!SubworldSystem.AnyActive())
		{
			foreach (NPC npc in _npcs)
			{
				DespawnNpc(npc);
			}

			return;
		}

		List<NPC> npcsToDespawn = [];
		foreach (NPC npc in _npcs)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Center.Distance(npc.Center) > 1800f) // TODO: spawn distance hard coded; make it smartly, somehow...
				{
					npcsToDespawn.Add(npc);
					break;
				}
			}
		}

		List<NPC> npcsToSpawn = [];
		foreach (KeyValuePair<NPC, Vector2> npc in _savedNpcs)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Center.Distance(npc.Value) < 1200f) // TODO: spawn distance hard coded; make it smartly, somehow...
				{
					npcsToSpawn.Add(npc.Key);
					break;
				}
			}
		}

		npcsToDespawn.ForEach(DespawnNpc);
		npcsToSpawn.ForEach(SpawnNpc);
	}

	// copied from NPC class
	private static int GetAvailableNPCSlot(int Type, int startIndex)
	{
		bool num = Type >= 0 && NPCID.Sets.SpawnFromLastEmptySlot[Type];
		int t = 200;
		int num2 = 1;
		if (num)
		{
			t--;
			Utils.Swap(ref startIndex, ref t);
			num2 = -1;
		}

		for (int i = startIndex; i != t; i += num2)
		{
			if (!Main.npc[i].active)
			{
				return i;
			}
		}

		for (int j = startIndex; j != t; j += num2)
		{
			if (Main.npc[j].CanBeReplacedByOtherNPCs)
			{
				return j;
			}
		}

		return -1;
	}

	private static readonly MethodInfo _method = typeof(NPCLoader).GetMethod("OnSpawn", BindingFlags.NonPublic | BindingFlags.Static);
	public static void MakeNPC(Vector2 center, int type, IEntitySource source = null)
	{
		NPC npc = new MapNPC();
		npc.SetDefaults(type);
		// npc.whoAmI = availableNPCSlot; || no slot for now
		// GiveTownUniqueDataToNPCsThatNeedIt(type, availableNPCSlot); || we wont be spawning town npcs, i think
		npc.Center = center;
		npc.active = true;
		// npc.timeLeft = int.MaxValue; || it wont despawn to inactivity
		npc.wet = Collision.WetCollision(npc.position, npc.width, npc.height);
		npc.ai[0] = 0f;
		npc.ai[1] = 0f;
		npc.ai[2] = 0f;
		npc.ai[3] = 0f;
		npc.target = 255;

		/*
		if (type == 50)
		{
			if (Main.netMode == 0)
			{
				Main.NewText(Language.GetTextValue("Announcement.HasAwoken", npc.typeName), 175, 75);
			}
			else if (Main.netMode == 2)
			{
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", npc.GettypeNetName()), new Color(175, 75, 255));
			}
		}
		*/

		// NPCLoader.OnSpawn(npc, source);
		_method.Invoke(null, [npc, source]);

		_savedNpcs.Add(npc, npc.Center);
	}
}

internal class MapNPC : NPC
{
	public new bool DoesntDespawnToInactivity()
	{
		return true;
	}
}