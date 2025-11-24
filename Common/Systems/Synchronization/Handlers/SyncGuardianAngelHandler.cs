using System.IO;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs the rings of the Guardian Angel's hits.<br/>Signature:<br/>
/// <c>byte playerWhoAmI, short npcWho, bool runLocally = false</c>
/// </summary>
internal class SyncGuardianAngelHandler : Handler
{
	public static void Send(byte playerWhoAmI, short npcWho, int toPlayer = -1, int ignorePlayer = -1, bool runLocally = false)
	{
		ModPacket packet = Networking.GetPacket<SyncGuardianAngelHandler>();
		packet.Write(playerWhoAmI);
		packet.Write(npcWho);
		packet.Send(toPlayer, ignorePlayer);

		if (runLocally)
		{
			HitGuardianAngel(playerWhoAmI, npcWho);
		}
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte who = reader.ReadByte();
		short npcWho = reader.ReadInt16();

		HitGuardianAngel(who, npcWho);

		ModPacket packet = Networking.GetPacket(Id);

		packet.Write(who);
		packet.Write(npcWho);
		packet.Send(-1, who);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		HitGuardianAngel(reader.ReadByte(), reader.ReadInt16());
	}

	public static void HitGuardianAngel(byte playerWhoAmI, short npcWho)
	{
		Main.npc[npcWho].GetGlobalNPC<GuardianAngel.AngelRingNPC>().ApplyRing(Main.npc[npcWho], playerWhoAmI, true);
	}
}
