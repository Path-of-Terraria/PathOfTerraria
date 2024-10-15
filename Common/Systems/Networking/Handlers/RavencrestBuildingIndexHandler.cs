using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.Experience;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class RavencrestBuildingIndex
{
	/// <summary>
	/// Spawns an orb on all clients but <paramref name="target"/>. 
	/// Generally, this should only be called by <see cref="ExperienceModSystem.SpawnExperience(int, Vector2, Vector2, int, bool)"/>.
	/// </summary>
	/// <param name="target">Target for the exp to aim for.</param>
	/// <param name="xpValue">Amount of XP to spawn.</param>
	/// <param name="position">Where it spawns.</param>
	/// <param name="velocity">The base velocity of the spawned exp.</param>
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
