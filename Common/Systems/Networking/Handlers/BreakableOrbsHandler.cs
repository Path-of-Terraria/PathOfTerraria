using PathOfTerraria.Common.Systems.VanillaModifications;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class BreakableOrbsHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.BreakableOrbs;

	/// <inheritdoc cref="Networking.Message.BreakableOrbs"/>
	public override void Send(params object[] parameters)
	{
		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		DisableOrbBreaking.BreakableOrbSystem.CanBreakOrb = true;
		NetMessage.SendData(MessageID.WorldData);
	}
}
