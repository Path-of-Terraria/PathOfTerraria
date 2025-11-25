using PathOfTerraria.Common.Systems.VanillaModifications;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Allows all players on the server to break Shadow Orbs and Crimson Hearts.
/// </summary>
internal class BreakableOrbsHandler : Handler
{
	public static void Send()
	{
		Networking.GetPacket<BreakableOrbsHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		DisableOrbBreaking.BreakableOrbSystem.CanBreakOrb = true;
		NetMessage.SendData(MessageID.WorldData);
	}
}
