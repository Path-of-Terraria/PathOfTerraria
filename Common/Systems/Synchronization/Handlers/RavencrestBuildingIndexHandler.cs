using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class RavencrestBuildingIndex : Handler
{
	/// <inheritdoc cref="Networking.Message.SetRavencrestBuildingIndex"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out string name, out int index);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(name);
		packet.Write((byte)index);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		string name = reader.ReadString();
		byte index = reader.ReadByte();

		RavencrestSystem.UpgradeBuilding(name, index);
	}
}
