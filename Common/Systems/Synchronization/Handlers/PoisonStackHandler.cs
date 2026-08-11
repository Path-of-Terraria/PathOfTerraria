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
		ModPacket packet = Networking.GetPacket<PoisonStackHandler>(9);
		packet.Write((short)npc.whoAmI);
		packet.Write(false);
		packet.Write(time);
		packet.Write(player is not null);
		packet.Send();
	}

	public static void SendExistingStack(NPC npc, int time, float damagePerTick, float tickRate, Player? player)
	{
		ModPacket packet = Networking.GetPacket<PoisonStackHandler>(17);
		packet.Write((short)npc.whoAmI);
		packet.Write(true);
		packet.Write(time);
		packet.Write(damagePerTick);
		packet.Write(tickRate);
		packet.Write(player is not null);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		short npcWhoAmI = reader.ReadInt16();
		bool isExistingStack = reader.ReadBoolean();
		int time = reader.ReadInt32();
		float damagePerTick = isExistingStack ? reader.ReadSingle() : 0f;
		float tickRate = isExistingStack ? reader.ReadSingle() : 0f;
		bool hasPlayerSource = reader.ReadBoolean();

		if (sender >= Main.maxPlayers || npcWhoAmI < 0 || npcWhoAmI >= Main.maxNPCs || time <= 0
			|| isExistingStack && (!float.IsFinite(damagePerTick) || damagePerTick < 0f || !float.IsFinite(tickRate) || tickRate <= 0f))
		{
			return;
		}

		Player? player = hasPlayerSource ? Main.player[sender] : null;
		NPC npc = Main.npc[npcWhoAmI];

		if (player is { active: false } || !npc.active)
		{
			return;
		}

		if (isExistingStack)
		{
			PoisonedDebuff.ApplyExistingStack(npc, time, damagePerTick, tickRate, player);
		}
		else
		{
			PoisonedDebuff.Apply(npc, time, player);
		}
	}
}
