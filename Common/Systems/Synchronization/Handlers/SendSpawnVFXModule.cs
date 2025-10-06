using PathOfTerraria.Content.Projectiles.Utility;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.SpawnVFX"/>
internal class SendSpawnVFXModule : Handler
{
	public enum VFXType : byte
	{
		/// <summary>
		/// Spawns a bunch of shimmer VFX. Used by <see cref="ExitPortal"/>'s item magnet functionality.
		/// </summary>
		ShimmerTeleport,
	}

	public override Networking.Message MessageType => Networking.Message.SpawnVFX;

	/// <inheritdoc cref="Networking.Message.SpawnVFX"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out Vector2 position, out VFXType type);

		ModPacket packet = Networking.GetPacket(MessageType, 6);
		packet.WriteVector2(position);
		packet.Write((byte)type);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(MessageType);
		packet.WriteVector2(reader.ReadVector2());
		packet.Write(reader.ReadByte());
		packet.Send();
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		Vector2 pos = reader.ReadVector2();
		VFXType vfx = (VFXType)reader.ReadByte();

		SpawnVFX(pos, vfx);
	}

	private static void SpawnVFX(Vector2 pos, VFXType vfx)
	{
		switch (vfx)
		{
			case VFXType.ShimmerTeleport:
				ExitPortal.SpawnShimmerTeleportVFX(pos);
				break;
		}
	}
}
