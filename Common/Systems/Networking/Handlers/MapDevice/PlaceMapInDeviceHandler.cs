using PathOfTerraria.Content.Tiles.Furniture;
using System.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Networking.Handlers.MapDevice;

internal class PlaceMapInDeviceHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncMapDevicePlaceMap;

	/// <inheritdoc cref="Networking.Message.SyncMapDevicePlaceMap"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte fromWho, out short itemId, out Point16 entityKey);

		ModPacket packet = Networking.GetPacket(Networking.Message.SyncMapDevicePlaceMap);
		packet.Write(fromWho);
		packet.Write(itemId);
		packet.Write(entityKey.X);
		packet.Write(entityKey.Y);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
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

	internal override void ClientRecieve(BinaryReader reader)
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
