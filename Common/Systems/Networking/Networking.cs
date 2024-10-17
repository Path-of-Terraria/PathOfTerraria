using System.IO;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Networking;

internal static class Networking
{
	public enum Message : byte
	{
		/// <summary>
		/// Spawns experience. Signature:<br/>
		/// <c>byte target, int xpValue, Vector2 position, Vector2 velocity, bool spawnLocally = false</c>
		/// </summary>
		SpawnExperience,

		/// <summary>
		/// Syncs the usage of hotbar potions. Signature:<br/>
		/// <c>byte playerWhoAmI, bool isHealingPotion, byte newValue, bool runLocally = false</c>
		/// </summary>
		SetHotbarPotionUse,

		/// <summary>
		/// Syncs the rings of the Guardian Angel's hits. Signature:<br/>
		/// <c>byte playerWhoAmI, short npcWho, bool runLocally = false</c>
		/// </summary>
		SyncGuardianAngelHit,

		/// <summary>
		/// Spawns an NPC on the server. Signatures:<br/>
		/// <c>short npcId, Vector2 position</c><br/>
		/// <c>short npcId, Vector2 position, Vector2 velocity</c>
		/// </summary>
		SpawnNPCOnServer,
	}

	internal static void HandlePacket(BinaryReader reader)
	{
		var message = (Message)reader.ReadByte();

		switch (message)
		{
			case Message.SpawnExperience:
				if (Main.netMode == NetmodeID.Server)
				{
					ExperienceHandler.ServerRecieve(reader);
				}
				else
				{
					ExperienceHandler.ServerRecieve(reader);
				}

				break;

			case Message.SetHotbarPotionUse:
				if (Main.netMode == NetmodeID.Server)
				{
					HotbarPotionHandler.ServerRecieve(reader);
				}
				else
				{
					HotbarPotionHandler.ClientRecieve(reader);
				}

				break;

			case Message.SyncGuardianAngelHit:
				if (Main.netMode == NetmodeID.Server)
				{
					SyncGuardianAngelHandler.ServerRecieve(reader);
				}
				else
				{
					SyncGuardianAngelHandler.ClientRecieve(reader);
				}

				break;

			case Message.SpawnNPCOnServer:
				if (Main.netMode == NetmodeID.Server)
				{
					SpawnNPCOnServerHandler.ServerRecieve(reader);
				}

				break;

			default:
				throw null;
		}
	}

	internal static ModPacket GetPacket(Message type)
	{
		ModPacket packet = PoTMod.Instance.GetPacket();
		packet.Write((byte)type);
		return packet;
	}
}
