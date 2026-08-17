using PathOfTerraria.Common.Systems.Synchronization;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

/// <summary>
/// Reconciles which other players the local client believes are in this world against the server's
/// view, and asks for a resync when they disagree.
/// <para/>
/// Subworld travel wipes every other player from the traveller's client - SubworldLibrary's
/// ExitWorldCallBack deactivates them all - and vanilla only ever re-sends them once, during the
/// rejoin handshake. If that single sync is missed, which two players transitioning at the same time
/// is enough to cause, the affected players stay invisible in world and on the map until someone
/// relogs. This is the safety net for that; the handshake bug itself lives in SubworldLibrary.
/// </summary>
internal class PlayerPresencePlayer : ModPlayer
{
	/// <summary> How often the local client re-checks the server's player list. </summary>
	private const int CheckInterval = 60 * 10;

	/// <summary>
	/// Minimum gap between resync requests. A resync re-sends every player's join data, so a mismatch
	/// that somehow never clears must not turn into one of those every <see cref="CheckInterval"/>.
	/// </summary>
	private const int ResyncCooldown = 60 * 30;

	private static int resyncCooldownTimer;

	private int checkTimer;

	/// <summary> Asks the server which players it considers present here, and answers with the list. </summary>
	public class PlayerPresenceHandler : Handler
	{
		public static void Send()
		{
			Networking.GetPacket<PlayerPresenceHandler>().Send();
		}

		internal override void ServerReceive(BinaryReader reader, byte sender)
		{
			List<byte> present = [];

			foreach (Player player in Main.ActivePlayers)
			{
				// Mirrors what vanilla's SyncConnectedPlayer would actually send: a player is only
				// visible to anyone else once their client has reached state 10.
				if (player.whoAmI != sender && Netplay.Clients[player.whoAmI].State == 10)
				{
					present.Add((byte)player.whoAmI);
				}
			}

			ModPacket packet = Networking.GetPacket(Id);
			packet.Write((byte)present.Count);

			foreach (byte whoAmI in present)
			{
				packet.Write(whoAmI);
			}

			packet.Send(sender);
		}

		internal override void ClientReceive(BinaryReader reader, byte sender)
		{
			byte count = reader.ReadByte();
			bool missing = false;

			for (int i = 0; i < count; i++)
			{
				byte whoAmI = reader.ReadByte();

				if (whoAmI < Main.maxPlayers && !Main.player[whoAmI].active)
				{
					missing = true;
				}
			}

			// Only the missing direction is corrected. Deactivating players the server did not list
			// would race against someone joining, and nothing would ever bring them back - which is
			// the exact failure being worked around here.
			if (missing && resyncCooldownTimer <= 0)
			{
				resyncCooldownTimer = ResyncCooldown;
				RequestPlayerResyncHandler.Send();
			}
		}
	}

	/// <summary> Asks the server to re-send every present player's join data to us. </summary>
	public class RequestPlayerResyncHandler : Handler
	{
		public static void Send()
		{
			Networking.GetPacket<RequestPlayerResyncHandler>().Send();
		}

		internal override void ServerReceive(BinaryReader reader, byte sender)
		{
			// SyncConnectedPlayer routes players that are not in state 10 through SyncOnePlayer's
			// disconnect branch, which broadcasts them as inactive to every client. Never call it for
			// a client that has not finished joining.
			if (Netplay.Clients[sender].State != 10)
			{
				return;
			}

			NetMessage.SyncConnectedPlayer(sender);
		}
	}

	public override void OnEnterWorld()
	{
		checkTimer = 0;
		resyncCooldownTimer = 0;

		if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
		{
			PlayerPresenceHandler.Send();
		}
	}

	public override void PostUpdate()
	{
		if (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI != Main.myPlayer)
		{
			return;
		}

		if (resyncCooldownTimer > 0)
		{
			resyncCooldownTimer--;
		}

		if (++checkTimer < CheckInterval)
		{
			return;
		}

		checkTimer = 0;
		PlayerPresenceHandler.Send();
	}
}
