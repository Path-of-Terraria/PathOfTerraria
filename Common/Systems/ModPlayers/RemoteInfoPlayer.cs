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
			Send(Main.LocalPlayer, returnToPlayer);
		}

		public static void Send(Player plr, short returnToPlayer = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) { return; }

			ModPacket packet = Networking.GetPacket<SendRemoteInfoHandler>();
			if (Main.netMode == NetmodeID.Server)
			{
				packet.Write((byte)plr.whoAmI);
			}
			packet.Write((byte)plr.GetModPlayer<ClassingPlayer>().Class);
			packet.Write((byte)plr.GetModPlayer<ExpModPlayer>().Level);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				packet.Write(returnToPlayer);
			}
			if (Main.netMode == NetmodeID.Server) { packet.Send(returnToPlayer); }
			else { packet.Send(); }
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			int who = Main.netMode == NetmodeID.Server ? sender : reader.ReadByte();
			Player plr = Main.player[who];
			plr.GetModPlayer<ClassingPlayer>().Class = (StarterClass)reader.ReadByte();
			plr.GetModPlayer<ExpModPlayer>().Level = reader.ReadByte();

			if (Main.netMode == NetmodeID.Server)
			{
				short returnToPlayer = reader.ReadInt16();

				Send(plr, returnToPlayer);
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
			if (Main.netMode == NetmodeID.Server)
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
