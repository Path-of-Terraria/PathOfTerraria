using PathOfTerraria.Content.Tiles.Furniture;
using System.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Networking.Handlers.MapDevice;

internal static class ConsumeMapDeviceHandler
{
	/// <summary>
	/// Map device packet <paramref name="target"/>. 
	/// Generally, this should only be called by <see cref="MapDevicePlaceable.TryPlaceMap"/>.
	/// </summary>
	/// <param name="fromWho">The person entering the map.</param>
	/// <param name="entityKey">The key for the entity</param>
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
		byte fromWho = reader.ReadByte();
		short x = reader.ReadInt16();
		short y = reader.ReadInt16();

		packet.Write(x);
		packet.Write(y);
		packet.Send(-1, fromWho);

		if (TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			mapEntity.ConsumeMap();
		}

		ModContent.GetInstance<PoTMod>().Logger.Debug($"GOT: x:{x} - y:{y} - from:{fromWho}");
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		short x = reader.ReadInt16();
		short y = reader.ReadInt16();

		ModContent.GetInstance<PoTMod>().Logger.Debug($"GOT: x:{x} - y:{y} - EXISTS:{TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity _)}");

		if (TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			mapEntity.ConsumeMap();
		}
	}
}
