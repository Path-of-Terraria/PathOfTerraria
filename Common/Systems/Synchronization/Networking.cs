using SubworldLibrary;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PathOfTerraria.Common.Systems.Synchronization;

internal static class Networking
{
	internal static void HandlePacket(BinaryReader reader, byte sender)
	{
		byte messageId = reader.ReadByte();
		Handler handler = Handler.HandlersById[messageId];
		handler.Receive(reader, sender);

#if DEBUG
		PoTMod.Instance.Logger.Debug($"[PoT] Network got: {messageId} (Handler: {handler.GetType().Name})");
#endif
		return;
	}

	/// <summary>
	/// Sends a packet handled by <typeparamref name="T"/>. Shorthand for <see cref="GetPacket(byte, byte)"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="capacity"></param>
	/// <returns></returns>
	internal static ModPacket GetPacket<T>(byte capacity = 255) where T : Handler
	{
		return GetPacket(ModContent.GetInstance<T>().Id, capacity);
	}

	internal static ModPacket GetPacket(byte messageId, byte capacity = 255)
	{
		ModPacket packet = PoTMod.Instance.GetPacket(capacity);
		packet.Write(messageId);
		return packet;
	}

	/// <summary>
	/// Marks packet as finished and returns its buffer.<br/>
	/// This strips off the header info from the front of the buffer as well.
	/// </summary>
	internal static byte[] GetFinalPacketBuffer(ModPacket packet, bool removeHeader = true)
	{
		[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "buf")]
		extern static ref byte[] Buffer(ModPacket packet);

		[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Finish")]
		extern static void Finish(ModPacket packet);

		Finish(packet);
		byte[] buffer = Buffer(packet);

		if (removeHeader)
		{
			StripHeader(ref buffer);
		}

		return buffer;
	}

	/// <summary>
	/// Wraps around <see cref="SubworldSystem.SendToAllSubservers(Mod, byte[])"/> while stripping the header and simplifies getting the buffer from the <see cref="ModPacket"/>.
	/// </summary>
	internal static void SendToAllSubservers(ModPacket packet, bool stripHeader = true)
	{
		SubworldSystem.SendToAllSubservers(PoTMod.Instance, GetFinalPacketBuffer(packet, stripHeader));
	}

	/// <summary>
	/// Takes a <see cref="ModPacket"/> and sends it to the main server. Behaves as if using packet.Send() otherwise.
	/// </summary>
	/// <param name="packet">The packet to forward.</param>
	internal static void SendPacketToMainServer(ModPacket packet, string debugSendMessage = "")
	{
		byte[] data = (packet.BaseStream as MemoryStream).GetBuffer();
		StripHeader(ref data);

#if DEBUG
		string text = "";

		foreach (byte b in data)
		{
			text += $"{b}, ";
		}

		text = text[..^2];
		PoTMod.Instance.Logger.Debug("Cross-server communication, bytes: " + debugSendMessage + text);
#endif

		SubworldSystem.SendToMainServer(PoTMod.Instance, data);
	}

	internal static void StripHeader(ref byte[] data)
	{
		// Mod packets have a header included in the buffer.
		// The values are as follows, obtained from SubworldSystem.GetPacketHeader

		//			array[0] = byte.MaxValue;
		//			array[1] = (byte)(size - 1);
		//			array[2] = (byte)(size - 1 >> 8);
		//			array[3] = byte.MaxValue;
		//			array[4] = (byte)mod;
		//			if (ModNet.NetModCount >= 256)
		//			{
		//				array[5] = (byte)(mod >> 8);
		//			}

		// From what I can tell, 0 is just a sanity check for the server to confirm the packet was sent properly.
		// 1 is the size of the packet,
		// 2 is the size of the packet, latter 8 bits
		// 3 is...another sanity check?
		// 4 is the net ID of the mod in question
		// if there are more than 255 mods enabled, 5 is the net ID of the mod in question, latter 8 bits

		// This header has to be removed for normal functionality when the buffer is handled directly, which is required for sending data to subworlds

		if (ModNet.NetModCount >= 256)
		{
			data = data[5..];
		}
		else
		{
			data = data[4..];
		}
	}
}
