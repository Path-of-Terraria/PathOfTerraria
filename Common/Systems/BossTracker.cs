using PathOfTerraria.Common.NPCs.GlobalNPCs;
using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems;

internal class BossTracker : ModSystem
{
	public static HashSet<int> CachedBossesDowned = [];
	public static BitsByte DownedFlags;
	public static bool SkipWoFBox;

	private static readonly MethodInfo DoDeathEventsInfo = typeof(NPC).GetMethod("DoDeathEvents", BindingFlags.Instance | BindingFlags.NonPublic);

	public static bool DownedEaterOfWorlds { get => DownedFlags[0]; set => DownedFlags[0] = value; }
	public static bool DownedBrainOfCthulhu { get => DownedFlags[1]; set => DownedFlags[1] = value; }

	public override void Load()
	{
		On_NPC.DoDeathEvents += HijackDeathEffects;
		On_NPC.CreateBrickBoxForWallOfFlesh += StopBrickBox;
	}

	public static void AddDowned(int id)
	{
		CachedBossesDowned.Add(id);

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.RequestWorldData, -1, -1, null, Main.myPlayer);
			}
			else
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}
	}

	private void StopBrickBox(On_NPC.orig_CreateBrickBoxForWallOfFlesh orig, NPC self)
	{
		if (SkipWoFBox)
		{
			return;
		}

		orig(self);
	}

	private void HijackDeathEffects(On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer)
	{
		if (SubworldSystem.Current is BossDomainSubworld && self.boss)
		{
			self.type = NPCID.None;
			self.boss = false;
		}
		else
		{
			OnDeathNPC.OnDeathEffects(self);
		}

		orig(self, closestPlayer);
	}

	public override void PreUpdateEntities()
	{
		if (SubworldSystem.Current is null && CachedBossesDowned.Count > 0)
		{
			foreach (int type in CachedBossesDowned)
			{
				var npc = new NPC();
				npc.SetDefaults(type);
				npc.Center = Main.player[0].Center;

				if (npc.type == NPCID.EaterofWorldsHead)
				{
					npc.boss = true;
				}

				SkipWoFBox = true;
				DoDeathEventsInfo.Invoke(npc, [Main.player[0]]);
				SkipWoFBox = false;
			}

			CachedBossesDowned.Clear();
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add(nameof(DownedFlags), (byte)DownedFlags);
		tag.Add(nameof(CachedBossesDowned), (int[])[.. CachedBossesDowned]);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		CachedBossesDowned.Clear();
		DownedFlags = tag.GetByte(nameof(DownedFlags));
		CachedBossesDowned = [.. tag.GetIntArray(nameof(CachedBossesDowned))];
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)DownedFlags);
		writer.Write(CachedBossesDowned.Count);

		foreach (int item in CachedBossesDowned)
		{
			writer.Write(item);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		DownedFlags = reader.ReadByte();
		CachedBossesDowned.Clear();

		int count = reader.ReadInt32();
		
		for (int i = 0; i < count; ++i)
		{
			CachedBossesDowned.Add(reader.ReadInt32());
		}
	}

	public class BossTrackerNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.type == NPCID.EaterofWorldsHead && NPC.CountNPCS(NPCID.EaterofWorldsBody) == 0)
			{
				// EoW is dumb and won't register as a boss before death so this is the workaround
				DownedEaterOfWorlds = true;

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}

			if (!npc.boss)
			{
				return;
			}

			if (npc.type == NPCID.BrainofCthulhu)
			{
				DownedBrainOfCthulhu = true;

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
	}
}
