using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

#nullable enable

/// <summary>
/// Requests that the server add a stack of Poison to an NPC, preserving whether the sending player was its source.
/// </summary>
internal class PoisonStackHandler : Handler
{
	public static void Send(NPC npc, int time, Player? player)
	{
		ModPacket packet = Networking.GetPacket<PoisonStackHandler>(8);
		packet.Write((short)npc.whoAmI);
		packet.Write(time);
		packet.Write(player is not null);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		short npcWhoAmI = reader.ReadInt16();
		int time = reader.ReadInt32();
		bool hasPlayerSource = reader.ReadBoolean();

		if (sender >= Main.maxPlayers || npcWhoAmI < 0 || npcWhoAmI >= Main.maxNPCs || time <= 0)
		{
			return;
		}

		Player? player = hasPlayerSource ? Main.player[sender] : null;
		NPC npc = Main.npc[npcWhoAmI];

		if (player is { active: false } || !npc.active)
		{
			return;
		}

		PoisonedDebuff.Apply(npc, time, player);
	}
}
