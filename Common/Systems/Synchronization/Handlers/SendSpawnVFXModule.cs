using PathOfTerraria.Content.Dusts;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Swamp.NPCs.SwampBoss;
using System.IO;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Spawns arbitrary, pre-defined VFX on all clients and server.<br/>
/// Used for code run on only one client that should be shown on all clients, such as projectile spawning VFX, or server-side operations.
/// </summary>
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

		/// <summary>
		/// Creates a burst of moss and <see cref="DustID.Venom"/>/<see cref="BrightVenomDust"/> dust.
		/// </summary>
		SpawnPoisonMoss,
	}

	public static void SendNPCGores(NPC npc)
	{
		ModPacket packet = Networking.GetPacket<SendSpawnVFXModule>(6);
		packet.Write((byte)VFXType.SpawnNPCGores);
		packet.Write((short)npc.whoAmI);
		packet.Send();
	}

	public static void Send(Vector2 position, VFXType type)
	{
		ModPacket packet = Networking.GetPacket<SendSpawnVFXModule>(6);
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

		if (vfx == VFXType.SpawnNPCGores)
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
		else
		{
			SpawnVFX(reader.ReadVector2(), vfx);
		}
	}

	private static void SpawnVFX(Vector2 pos, VFXType vfx)
	{
		switch (vfx)
		{
			case VFXType.ShimmerTeleport:
				ExitPortal.SpawnShimmerTeleportVFX(pos);
				break;

			case VFXType.SpawnPoisonMoss:
				Mossling.SpawnPoisonMossVFX(pos);
				break;
		}
	}
}
