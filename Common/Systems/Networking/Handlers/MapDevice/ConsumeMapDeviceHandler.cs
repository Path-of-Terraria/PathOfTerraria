using PathOfTerraria.Content.Tiles.Furniture;
using System.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Networking.Handlers.MapDevice;

internal static class ConsumeMapDeviceHandler
{
	/// <summary>
	/// Spawns an orb on all clients but <paramref name="target"/>. 
	/// Generally, this should only be called by <see cref="ExperienceModSystem.SpawnExperience(int, Vector2, Vector2, int, bool)"/>.
	/// </summary>
	/// <param name="target">Target for the exp to aim for.</param>
	/// <param name="xp">Amount of XP to spawn.</param>
	/// <param name="spawn">Where it spawns.</param>
	/// <param name="velocity">The base velocity of the spawned exp.</param>
	public static void Send(byte fromWho, Point16 entityKey)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.ConsumeMapOffOfDevice);

		packet.Write(fromWho);
		packet.Write(entityKey.X);
		packet.Write(entityKey.Y);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.ConsumeMapOffOfDevice);
		short fromWho = reader.ReadByte();
		short x = reader.ReadInt16();
		short y = reader.ReadInt16();

		packet.Write(x);
		packet.Write(y);
		packet.Send(-1, fromWho);
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		short x = reader.ReadInt16();
		short y = reader.ReadInt16();

		if (TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			mapEntity.ConsumeMap();
		}
	}
}
