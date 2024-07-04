using PathOfTerraria.Core.Systems.MobSystem;
using System.IO;

namespace PathOfTerraria.Core.Systems.Networking.Handlers;

internal static class MobRarityHandler
{
	public static void Send(short npcWho, byte rarity, bool runLocally = false)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.ApplyMobRarity);

		packet.Write(npcWho);
		packet.Write(rarity);
		packet.Send();

		if (runLocally)
		{
			ApplyRarity(npcWho, rarity);
		}
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		short npcWho = reader.ReadInt16();
		byte rarity = reader.ReadByte();

		ModPacket packet = Networking.GetPacket(Networking.Message.ApplyMobRarity);

		packet.Write(npcWho);
		packet.Write(rarity);
		packet.Send();

		ApplyRarity(npcWho, rarity);
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		ApplyRarity(reader.ReadInt16(), reader.ReadByte());
	}

	public static void ApplyRarity(short npcWho, byte rarity)
	{
		NPC npc = Main.npc[npcWho];
		MobAprgSystem mob = npc.GetGlobalNPC<MobAprgSystem>();
		mob.Rarity = (Rarity)rarity;
		mob.ApplyRarity(Main.npc[npcWho], true);
	}
}
