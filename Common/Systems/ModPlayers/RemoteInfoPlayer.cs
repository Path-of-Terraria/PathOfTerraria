using PathOfTerraria.Common.Classing;
using PathOfTerraria.Common.Systems.Synchronization;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Handles requesting certain information that is visual-only.
/// </summary>
internal class RemoteInfoPlayer : ModPlayer
{
	/// <summary>
	/// Sends all "visual info" to other clients. This info isn't usually used for multiplayer gameplay otherwise.
	/// </summary>
	public class SendRemoteInfoHandler : Handler
	{
		public static void Send(short returnToPlayer = -1)
		{
			Player plr = Main.LocalPlayer;

			ModPacket packet = Networking.GetPacket<SendRemoteInfoHandler>();
			packet.Write((byte)plr.GetModPlayer<ClassingPlayer>().Class);
			packet.Write((byte)plr.GetModPlayer<ExpModPlayer>().Level);
			packet.Write(returnToPlayer);
			packet.Send();
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			int who = Main.dedServ ? sender : reader.ReadByte();
			Player plr = Main.player[who];
			plr.GetModPlayer<ClassingPlayer>().Class = (StarterClass)reader.ReadByte();
			plr.GetModPlayer<ExpModPlayer>().Level = reader.ReadByte();

			if (Main.dedServ)
			{
				short returnToPlayer = reader.ReadInt16();

				ModPacket packet = Networking.GetPacket<SendRemoteInfoHandler>();
				packet.Write((byte)plr.whoAmI);
				packet.Write((byte)plr.GetModPlayer<ClassingPlayer>().Class);
				packet.Write((byte)plr.GetModPlayer<ExpModPlayer>().Level);
				packet.Send(returnToPlayer);
			}
		}
	}

	/// <summary>
	/// Requests a <see cref="SendRemoteInfoHandler"/> from every other joined player.
	/// </summary>
	public class RequestRemoteInfoHandler : Handler
	{
		public static void Send()
		{
			ModPacket packet = Networking.GetPacket<RequestRemoteInfoHandler>();
			packet.Send();
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			if (Main.dedServ)
			{
				ModPacket packet = Networking.GetPacket<RequestRemoteInfoHandler>();
				packet.Write(sender);
				packet.Send();
			}
			else
			{
				SendRemoteInfoHandler.Send(reader.ReadByte());
			}
		}
	}

	public override void OnEnterWorld()
	{
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			return;
		}

		RequestRemoteInfoHandler.Send();
		SendRemoteInfoHandler.Send();
	}
}
