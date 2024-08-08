using PathOfTerraria.Content.Tiles.Furniture;
using System.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Networking.Handlers.MapDevice;

internal static class PlaceMapInDeviceHandler
{
	/// <summary>
	/// Sets the item in a MapDeviceEntity at <paramref name="entityKey"/> to the given item.
	/// </summary>
	/// <param name="fromWho">The player who sent the packet.</param>
	/// <param name="itemId">The item ID to place into the map entity.</param>
	/// <param name="entityKey">The position of the entity.</param>
	public static void Send(byte fromWho, short itemId, Point16 entityKey)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncMapDevicePlaceMap);

		packet.Write(fromWho);
		packet.Write(itemId);
		packet.Write(entityKey.X);
		packet.Write(entityKey.Y);
		packet.Send();
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SyncMapDevicePlaceMap);
		short fromWho = reader.ReadByte();
		short itemId = reader.ReadInt16();
		short x = reader.ReadInt16();
		short y = reader.ReadInt16();

		packet.Write(itemId);
		packet.Write(x);
		packet.Write(y);
		packet.Send(-1, fromWho);

		if (TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			mapEntity.SetItem(itemId);
		}
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		short itemId = reader.ReadInt16();
		short x = reader.ReadInt16();
		short y = reader.ReadInt16();

		if (TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			mapEntity.SetItem(itemId);
		}
	}
}
