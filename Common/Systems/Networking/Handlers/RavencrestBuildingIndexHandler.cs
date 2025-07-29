using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class RavencrestBuildingIndex : Handler
{
	public override Networking.Message MessageType => Networking.Message.SetRavencrestBuildingIndex;

	/// <inheritdoc cref="Networking.Message.SetRavencrestBuildingIndex"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out string name, out int index);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(name);
		packet.Write((byte)index);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		string name = reader.ReadString();
		byte index = reader.ReadByte();

		RavencrestSystem.UpgradeBuilding(name, index);
	}
}
