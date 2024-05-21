using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Networking
{
	public static class Networking {
		private enum Message{
			SpawnExperienceOrb
		}

		public static void HandlePacket(BinaryReader reader, int sender){
			var message = (Message)reader.ReadByte();

			switch(message){
				case Message.SpawnExperienceOrb:
					ReceiveSpawnExperienceOrb(reader, sender);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void SendSpawnExperienceOrbs(int sender, int target, int xp, Vector2 spawn, float velocityLength){
			if(xp <= 0)
				return;

			ModPacket packet = GetPacket(Message.SpawnExperienceOrb);

			packet.Write((byte)target);
			packet.Write(xp);
			packet.WriteVector2(spawn);
			packet.Write(velocityLength);

			packet.Send(ignoreClient: sender);
		}

		private static void ReceiveSpawnExperienceOrb(BinaryReader reader, int sender){
			byte target = reader.ReadByte();
			int xp = reader.ReadInt32();
			Vector2 spawn = reader.ReadVector2();
			float velocityLength = reader.ReadSingle();

			if (Main.netMode == NetmodeID.Server)
			{
				SendSpawnExperienceOrbs(sender, target, xp, spawn, velocityLength);	
			}
			else
			{
				ExperienceTracker.SpawnExperience(xp, spawn, velocityLength, target);	
			}
		}

		private static ModPacket GetPacket(Message type){
			ModPacket packet = PathOfTerraria.Instance.GetPacket();
			packet.Write((byte)type);
			return packet;
		}
	}
}