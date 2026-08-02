using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Requests that the server add a stack of Poison to an NPC for the sending player.
/// </summary>
internal class PoisonStackHandler : Handler
{
	public static void Send(NPC npc, int time)
	{
		ModPacket packet = Networking.GetPacket<PoisonStackHandler>(7);
		packet.Write((short)npc.whoAmI);
		packet.Write(time);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		short npcWhoAmI = reader.ReadInt16();
		int time = reader.ReadInt32();

		if (sender >= Main.maxPlayers || npcWhoAmI < 0 || npcWhoAmI >= Main.maxNPCs || time <= 0)
		{
			return;
		}

		Player player = Main.player[sender];
		NPC npc = Main.npc[npcWhoAmI];

		if (!player.active || !npc.active)
		{
			return;
		}

		PoisonedDebuff.Apply(npc, time, player);
	}
}
