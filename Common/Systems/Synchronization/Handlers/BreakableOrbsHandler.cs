using PathOfTerraria.Common.Systems.VanillaModifications;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Allows all players on the server to break Shadow Orbs and Crimson Hearts.
/// </summary>
internal class BreakableOrbsHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.BreakableOrbs"/>
	public override void Send(params object[] parameters)
	{
		ModPacket packet = Networking.GetPacket(Id);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		DisableOrbBreaking.BreakableOrbSystem.CanBreakOrb = true;
		NetMessage.SendData(MessageID.WorldData);
	}
}
