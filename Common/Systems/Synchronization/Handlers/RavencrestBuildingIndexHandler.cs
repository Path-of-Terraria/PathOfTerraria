using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Sets the index of a given Ravencrest structure.
/// </summary>
internal class RavencrestBuildingIndex : Handler
{
	public static void Send(string name, int index)
	{
		ModPacket packet = Networking.GetPacket<RavencrestBuildingIndex>();
		packet.Write(name);
		packet.Write(index);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		string name = reader.ReadString();
		int index = reader.ReadInt32();

		RavencrestSystem.UpgradeBuilding(name, index);
	}
}
