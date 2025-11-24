using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization;

/// <summary>
/// Defines a handler for a specific packet type. The packet is automatically assigned an ID for IO.<br/>
/// This recieves a packet and runs <see cref="Receive(BinaryReader, byte)"/>.<br/>
/// This simplifies creating and sending custom packets.
/// </summary>
internal abstract class Handler : ILoadable
{
	public static Dictionary<byte, Handler> HandlersById = [];

	/// <summary>
	/// The automatically assigned ID for this packet. Set in <see cref="Load(Mod)"/>.
	/// </summary>
	public byte Id { get; private set; }

	/// <summary>  Handles the server receiving the packet. This is required as the majority of packets are sent to the server.  </summary>
	internal virtual void ServerReceive(BinaryReader reader, byte sender)
	{
	}

	/// <summary> Handles a client receiving the packet. This is not required since it is not uncommon for packets to not have clients recieve them. </summary>
	internal virtual void ClientReceive(BinaryReader reader, byte sender)
	{
	}

	/// <summary> Handles receives the packet for both clients and servers. Defaults to calling ClientReceive or ServerReceive. </summary>
	internal virtual void Receive(BinaryReader reader, byte sender)
	{
		if (Main.netMode == NetmodeID.Server)
		{
			ServerReceive(reader, sender);
		}
		else
		{
			ClientReceive(reader, sender);
		}
	}

	public void Load(Mod mod)
	{
		// Switch to 7-bit-encoded Int32 if this panics:
		System.Diagnostics.Debug.Assert(HandlersById.Count < byte.MaxValue);

		Id = (byte)HandlersById.Count;
		HandlersById.Add(Id, this);

		OnLoad(mod);
	}

	public virtual void OnLoad(Mod mod) { } 

	public virtual void Unload()
	{
		HandlersById.Remove(Id);
	}
}
