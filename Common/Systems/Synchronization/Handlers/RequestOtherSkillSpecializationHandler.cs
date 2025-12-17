using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Requests all other players to sync their skill specializations. Should be sent from the client only.
/// </summary>
internal class RequestOtherSkillSpecializationHandler : Handler
{
	public static void Send()
	{
		Networking.GetPacket<RequestOtherSkillSpecializationHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		packet.Send(-1, sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		Main.LocalPlayer.GetModPlayer<SkillTreePlayer>().SyncAllSpecializations();
	}
}
