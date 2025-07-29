using PathOfTerraria.Content.Tiles.Furniture;
using System.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Networking.Handlers.MapDevice;

internal class ConsumeMapDeviceHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.ConsumeMapOffOfDevice;

	/// <summary>
	/// Map device packet <paramref name="target"/>. 
	/// Generally, this should only be called by <see cref="MapDevicePlaceable.TryPlaceMap"/>.
	/// </summary>
	/// <param name="fromWho">The person entering the map.</param>
	/// <param name="entityKey">The key for the entity</param>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte fromWho, out Point16 entityKey);

		ModPacket packet = Networking.GetPacket(Networking.Message.ConsumeMapOffOfDevice);
		packet.Write(fromWho);
		packet.Write(entityKey.X);
		packet.Write(entityKey.Y);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
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
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		short x = reader.ReadInt16();
		short y = reader.ReadInt16();

		if (TileEntity.ByPosition.TryGetValue(new(x, y), out TileEntity tileEntity) && tileEntity is MapDeviceEntity mapEntity)
		{
			mapEntity.ConsumeMap();
		}
	}
}
