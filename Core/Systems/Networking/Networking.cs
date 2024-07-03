using PathOfTerraria.Core.Systems.Networking.Handlers;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Networking;

internal static class Networking
{
	public enum Message : byte
	{
		SpawnExperience,
		SetHotbarPotionUse,
	}

	internal static void HandlePacket(BinaryReader reader, int whoAmI)
	{
		var message = (Message)reader.ReadByte();

		switch (message)
		{
			case Message.SpawnExperience:
				if (Main.netMode == NetmodeID.Server)
				{
					ExperienceHandler.ServerRecieveExperience(reader);
				}
				else
				{
					ExperienceHandler.ClientRecieveExperience(reader);
				}

				break;

			case Message.SetHotbarPotionUse:
				if (Main.netMode == NetmodeID.Server)
				{
					HotbarPotionHandler.ServerRecieveHotbarPotion(reader);
				}
				else
				{
					HotbarPotionHandler.ClientRecieve(reader);
				}

				break;

			default:
				throw null;
		}
	}

	internal static ModPacket GetPacket(Message type)
	{
		ModPacket packet = PathOfTerraria.Instance.GetPacket();
		packet.Write((byte)type);
		return packet;
	}
}
