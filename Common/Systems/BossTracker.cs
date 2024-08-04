using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems;

internal class BossTracker : ModSystem
{
	public static BitsByte DownedFlags = new();

	public static bool DownedEaterOfWorlds { get => DownedFlags[0]; set => DownedFlags[0] = value; }
	public static bool DownedBrainOfCthulhu { get => DownedFlags[1]; set => DownedFlags[1] = value; }

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add(nameof(DownedFlags), (byte)DownedFlags);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		DownedFlags = tag.GetByte(nameof(DownedFlags));
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)DownedFlags);
	}

	public override void NetReceive(BinaryReader reader)
	{
		DownedFlags = reader.ReadByte();
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
