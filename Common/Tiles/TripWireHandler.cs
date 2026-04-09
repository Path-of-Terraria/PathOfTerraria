using PathOfTerraria.Common.Systems.Synchronization;
using System.IO;

namespace PathOfTerraria.Common.Tiles;

/// <summary>
/// Calls <see cref="Wiring.TripWire(int, int, int, int)"/> on the server, with optional forwarding (which may be redundant).<br/>
/// I could not find an equivalent vanilla packet, but there may be one. - Gabe
/// </summary>
internal class TripWireHandler : Handler
{
	public static void Send(int i, int j, bool forward = true)
	{
		ModPacket packet = Networking.GetPacket<TripWireHandler>();
		packet.Write((short)i);
		packet.Write((short)j);
		packet.Write(forward);
		packet.Send();
	}

	internal override void Receive(BinaryReader reader, byte sender)
	{
		int i = reader.ReadInt16();
		int j = reader.ReadInt16();
		bool forward = reader.ReadBoolean();

		Wiring.TripWire(i, j, 1, 1);

		if (forward && Main.dedServ)
		{
			Send(i, j, forward);
		}
	}
}
