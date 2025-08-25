using System.Collections.Generic;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization;

/// <summary>
/// Defines a handler of a given <see cref="Networking.Message"/>. 
/// That is, something that recieves a packet and runs either <see cref="ServerRecieve(BinaryReader)"/> or <see cref="ClientRecieve(BinaryReader)"/> automatically.<br/>
/// This simplifies creating and sending custom packets.
/// </summary>
internal abstract class Handler : ILoadable
{
	public static Dictionary<Networking.Message, Handler> HandlerForMessage = [];

	/// <summary>
	/// The message type this handler is associated with.
	/// </summary>
	public abstract Networking.Message MessageType { get; }

	/// <summary>
	/// Sends the packet with the given parameters. Ideally, write in docs to clarify the signature so it's easier to use, 
	/// or use inheritdoc for the <see cref="Networking.Message"/> so both have the same comment for consistency and readability.
	/// </summary>
	/// <param name="parameters">Parameters to use for the method.</param>
	public abstract void Send(params object[] parameters);

	/// <summary>
	/// Handles the server recieving the packet. This is required as the majority of packets are sent to the server.
	/// </summary>
	internal abstract void ServerRecieve(BinaryReader reader);

	/// <summary>
	/// Handles a client recieving the packet. This is not required since it is not uncommon for packets to not have clients recieve them.
	/// </summary>
	internal virtual void ClientRecieve(BinaryReader reader) { }

	public void Load(Mod mod)
	{
		HandlerForMessage.Add(MessageType, this);
	}

	public void Unload()
	{
		HandlerForMessage.Remove(MessageType);
	}

	public static bool TryGetOptionalValue<T>(object[] objects, int length, out T value)
	{
		if (objects.Length > length && objects[length] is T option)
		{
			value = option;
			return true;
		}

		value = default;
		return false;
	}

	public static void CastParameters<T>(object[] objects, out T one)
	{
		one = TryCast<T>(objects[0]);
	}

	public static void CastParameters<T1, T2>(object[] objects, out T1 one, out T2 two)
	{
		one = TryCast<T1>(objects[0]);
		two = TryCast<T2>(objects[1]);
	}

	public static void CastParameters<T1, T2, T3>(object[] objects, out T1 one, out T2 two, out T3 three)
	{
		one = TryCast<T1>(objects[0]);
		two = TryCast<T2>(objects[1]);
		three = TryCast<T3>(objects[2]);
	}

	public static void CastParameters<T1, T2, T3, T4>(object[] objects, out T1 one, out T2 two, out T3 three, out T4 four)
	{
		one = TryCast<T1>(objects[0]);
		two = TryCast<T2>(objects[1]);
		three = TryCast<T3>(objects[2]);
		four = TryCast<T4>(objects[3]);
	}

	public static void CastParameters<T1, T2, T3, T4, T5>(object[] objects, out T1 one, out T2 two, out T3 three, out T4 four, out T5 five)
	{
		one = TryCast<T1>(objects[0]);
		two = TryCast<T2>(objects[1]);
		three = TryCast<T3>(objects[2]);
		four = TryCast<T4>(objects[3]);
		five = TryCast<T5>(objects[4]);
	}

	public static void CastParameters<T1, T2, T3, T4, T5, T6>(object[] objects, out T1 one, out T2 two, out T3 three, out T4 four, out T5 five, out T6 six)
	{
		one = TryCast<T1>(objects[0]);
		two = TryCast<T2>(objects[1]);
		three = TryCast<T3>(objects[2]);
		four = TryCast<T4>(objects[3]);
		five = TryCast<T5>(objects[4]);
		six = TryCast<T6>(objects[5]);
	}

	public static void CastParameters<T1, T2, T3, T4, T5, T6, T7>(object[] objects, out T1 one, out T2 two, out T3 three, out T4 four, out T5 five, out T6 six, out T7 sev)
	{
		one = TryCast<T1>(objects[0]);
		two = TryCast<T2>(objects[1]);
		three = TryCast<T3>(objects[2]);
		four = TryCast<T4>(objects[3]);
		five = TryCast<T5>(objects[4]);
		six = TryCast<T6>(objects[5]);
		sev = TryCast<T7>(objects[6]);
	}

	private static T TryCast<T>(object parameter)
	{
		if (parameter is T t)
		{
			return t;
		}

		throw new ArgumentException("parameter is not type " + typeof(T).Name + "!");
	}
}
