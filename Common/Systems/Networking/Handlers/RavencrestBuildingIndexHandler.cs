using PathOfTerraria.Common.Subworlds.RavencrestContent;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class RavencrestBuildingIndex
{
	/// <summary>
	/// Sets the building index of the given Ravencrest building.
	/// This should be called instead of <see cref="RavencrestSystem.UpgradeBuilding(string, int)"/> on multiplayer clients.
	/// </summary>
	/// <param name="name">Name of the building.</param>
	/// <param name="index">Index to use.</param>
	public static void Send(string name, int index)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SetRavencrestBuildingIndex);
		packet.Write(name);
		packet.Write((byte)index);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		string name = reader.ReadString();
		byte index = reader.ReadByte();

		RavencrestSystem.UpgradeBuilding(name, index);
	}
}
