using System.IO;
using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncGuardianAngelHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.SyncGuardianAngelHit"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte playerWhoAmI, out short npcWho);

		ModPacket packet = Networking.GetPacket(Id);

		packet.Write(playerWhoAmI);
		packet.Write(npcWho);
		packet.Send();

		if (TryGetOptionalValue(parameters, 2, out bool runLocally) && runLocally)
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
