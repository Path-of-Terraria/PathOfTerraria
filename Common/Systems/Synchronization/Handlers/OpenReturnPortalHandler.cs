using PathOfTerraria.Common.Subworlds;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal sealed class OpenReturnPortalHandler : Handler
{
	public static void Send()
	{
		ModPacket packet = Networking.GetPacket<OpenReturnPortalHandler>();
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		if (sender >= Main.maxPlayers)
		{
			return;
		}

		Player player = Main.player[sender];

		if (!player.active)
		{
			return;
		}

		ReturnPortalPlayer.TryOpenReturnPortal(player);
	}
}