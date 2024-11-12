using PathOfTerraria.Common.Systems.Experience;
using PathOfTerraria.Content.NPCs.Town;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class MorvenFollowHandler
{
	public static void Send(byte player, byte who)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.MorvenFollow);

		packet.Write(player);
		packet.Write(who);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		byte player = reader.ReadByte();
		int who = reader.ReadByte();

		NPC npc = Main.npc[who];

		if (!npc.active || npc.ModNPC is not MorvenNPC morven)
		{
			return;
		}

		morven.SetFollow(player);
		npc.netUpdate = true;
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		int target = reader.ReadByte();
		int xp = reader.ReadInt32();
		Vector2 position = reader.ReadVector2();
		Vector2 velocity = reader.ReadVector2();

		ExperienceTracker.SpawnExperience(xp, position, velocity, target, true);
	}
}
