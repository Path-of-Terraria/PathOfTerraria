using System.IO;
using PathOfTerraria.Content.Tiles.Furniture;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal sealed class CloseMapDevicePortalHandler : Handler
{
	public static void Send(Point16 position)
	{
		ModPacket packet = Networking.GetPacket<CloseMapDevicePortalHandler>(5);
		packet.Write(position.X);
		packet.Write(position.Y);
		packet.Send();
	}

	public static bool TryClosePortal(Point16 position)
	{
		if (!TileEntity.ByPosition.TryGetValue(position, out TileEntity tileEntity) || tileEntity is not MapDeviceEntity device)
		{
			return false;
		}

		return device.TryClosingPortal();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		Point16 position = new(reader.ReadInt16(), reader.ReadInt16());
		TryClosePortal(position);
	}
}
