using PathOfTerraria.Content.NPCs.BossDomain.Mech;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs a player being grabbed by a <see cref="Grabber"/> NPC.
/// </summary>
internal class SyncPlayerGrabbed : Handler
{
	public static void Send(short npc, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<SyncPlayerGrabbed>(3);
		packet.Write(npc);
		packet.Send(toClient, ignoreClient);
	}

	internal override void Receive(BinaryReader reader, byte sender)
	{
		short npc = reader.ReadInt16();
		Main.player[sender].GetModPlayer<GrabberPlayer>().BeingGrabbed = npc;
		NPC grabber = Main.npc[npc];
		grabber.ai[2] = 0;
		grabber.ai[3] = sender;

		if (Main.netMode == NetmodeID.Server)
		{
			Send(npc, -1, sender);
		}
	}
}