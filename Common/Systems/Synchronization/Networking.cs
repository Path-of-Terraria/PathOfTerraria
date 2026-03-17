using System.IO;
using System.Runtime.CompilerServices;
using SubworldLibrary;

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

	/// <summary> Marks packet as finished and returns its buffer. </summary>
	internal static byte[] GetFinalPacketBuffer(ModPacket packet)
	{
		[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "buf")]
		extern static ref byte[] Buffer(ModPacket packet);

		[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Finish")]
		extern static void Finish(ModPacket packet);

		Finish(packet);
		return Buffer(packet);
	}

	/// <summary>
	/// Takes a <see cref="ModPacket"/> and sends it to the main server. Behaves as if using packet.Send() otherwise.
	/// </summary>
	/// <param name="packet">The packet to forward.</param>
	internal static void SendPacketToMainServer(ModPacket packet, string debugSendMessage = "")
	{
		byte[] data = (packet.BaseStream as MemoryStream).GetBuffer();
		data = data[4..]; // Packets have a bunch of garbage data for some reason?

#if DEBUG
		string text = "";

		foreach (byte b in data)
		{
			text += $"{b}, ";
		}

		text = text[..^2];
		PoTMod.Instance.Logger.Debug(debugSendMessage + text);
#endif

		SubworldSystem.SendToMainServer(PoTMod.Instance, data);
	}
}
