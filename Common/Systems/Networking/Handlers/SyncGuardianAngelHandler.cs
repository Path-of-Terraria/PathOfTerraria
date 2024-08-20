using System.IO;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class SyncGuardianAngelHandler
{
	public static void Send(byte playerWhoAmI, short npcWho, bool runLocally = false)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncGuardianAngelHit);

		packet.Write(playerWhoAmI);
		packet.Write(npcWho);
		packet.Send();

		if (runLocally)
		{
			HitGuardianAngel(playerWhoAmI, npcWho);
		}
	}

	internal static void ServerReceive(BinaryReader reader)
	{
		byte who = reader.ReadByte();
		short npcWho = reader.ReadInt16();

		HitGuardianAngel(who, npcWho);

		ModPacket packet = Networking.GetPacket(Networking.Message.SyncGuardianAngelHit);

		packet.Write(who);
		packet.Write(npcWho);
		packet.Send(-1, who);
	}
	
	internal static void ClientReceive(BinaryReader reader)
	{
		HitGuardianAngel(reader.ReadByte(), reader.ReadInt16());
	}

	public static void HitGuardianAngel(byte playerWhoAmI, short npcWho)
	{
		Main.npc[npcWho].GetGlobalNPC<GuardianAngel.AngelRingNPC>().ApplyRing(Main.npc[npcWho], playerWhoAmI, true);
	}
}
