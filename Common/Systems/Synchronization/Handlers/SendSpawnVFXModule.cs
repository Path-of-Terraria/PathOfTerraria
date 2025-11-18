using PathOfTerraria.Content.Projectiles.Utility;
using System.IO;
using Terraria.Audio;

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

		/// <summary>
		/// Calls npc.HitEffect on the remote client. Takes an int instead of a Vector2.
		/// </summary>
		SpawnNPCGores,
	}

	public static void SendNPCGores(NPC npc)
	{
		ModPacket packet = Networking.GetPacket<SendSpawnVFXModule>(6);
		packet.Write((byte)VFXType.SpawnNPCGores);
		packet.Write((short)npc.whoAmI);
		packet.Send();
	}

	/// <inheritdoc cref="Networking.Message.SpawnVFX"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out Vector2 position, out VFXType type);

		ModPacket packet = Networking.GetPacket(Id, 6);
		packet.Write((byte)type);
		packet.WriteVector2(position);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);

		byte type = reader.ReadByte();
		packet.Write(reader.ReadByte());

		if ((VFXType)type != VFXType.SpawnNPCGores)
		{
			packet.WriteVector2(reader.ReadVector2());
		}
		else
		{
			packet.Write(reader.ReadInt16());
		}

		packet.Send();
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		var vfx = (VFXType)reader.ReadByte();

		if (vfx != VFXType.SpawnNPCGores)
		{
			SpawnVFX(reader.ReadVector2(), vfx);
		}
		else
		{
			short npcWho = reader.ReadInt16();
			NPC npc = Main.npc[npcWho];

			npc.active = true;
			npc.HitEffect(0, 1, false);

			if (npc.DeathSound is not null)
			{
				SoundEngine.PlaySound(npc.DeathSound, npc.Center);
			}

			npc.active = false;
		}
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
