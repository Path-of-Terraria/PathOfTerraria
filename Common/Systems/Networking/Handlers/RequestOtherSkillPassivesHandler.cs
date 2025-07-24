using PathOfTerraria.Common.Systems.Skills;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class RequestOtherSkillPassivesHandler
{
	public static void Send(byte player)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.RequestOthersSkillPassives);
		packet.Write(player);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.RequestOthersSkillPassives);
		byte target = reader.ReadByte();

		packet.Write(target);
		packet.Send(-1, target);
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		byte target = reader.ReadByte();

		if (Main.myPlayer == target)
		{
			Main.LocalPlayer.GetModPlayer<SkillTreePlayer>().SyncAllPassives();
		}
	}
}
