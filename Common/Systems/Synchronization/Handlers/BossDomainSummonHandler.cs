using System.IO;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class BossDomainSummonHandler : Handler
{
	internal enum BossDomainSummon : byte
	{
		QueenBee,
		Destroyer,
		Cultist
	}

	public static bool TrySummon(BossDomainSummon summon, Player player)
	{
		if (!CanSummon(summon, player))
		{
			return false;
		}

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			ModPacket packet = Networking.GetPacket<BossDomainSummonHandler>();
			packet.Write((byte)summon);
			packet.Send();
		}
		else
		{
			Spawn(summon, player.whoAmI);
		}

		return true;
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		BossDomainSummon summon = (BossDomainSummon)reader.ReadByte();

		if (sender >= Main.maxPlayers)
		{
			return;
		}

		Player player = Main.player[sender];

		if (!CanSummon(summon, player))
		{
			return;
		}

		Spawn(summon, sender);
	}

	private static bool CanSummon(BossDomainSummon summon, Player player)
	{
		if (player is null || !player.active || player.dead)
		{
			return false;
		}

		return summon switch
		{
			BossDomainSummon.QueenBee => SubworldSystem.Current is QueenBeeDomain && !NPC.AnyNPCs(NPCID.QueenBee),
			BossDomainSummon.Destroyer => SubworldSystem.Current is DestroyerDomain && !AnyDestroyerSegment(),
			BossDomainSummon.Cultist => SubworldSystem.Current is CultistDomain && !NPC.AnyNPCs(NPCID.CultistBoss),
			_ => false
		};
	}

	private static bool AnyDestroyerSegment()
	{
		return NPC.AnyNPCs(NPCID.TheDestroyer) || NPC.AnyNPCs(NPCID.TheDestroyerBody) || NPC.AnyNPCs(NPCID.TheDestroyerTail);
	}

	private static void Spawn(BossDomainSummon summon, int playerIndex)
	{
		Player player = Main.player[playerIndex];

		switch (summon)
		{
			case BossDomainSummon.QueenBee:
				NPC.SpawnOnPlayer(playerIndex, NPCID.QueenBee);
				break;

			case BossDomainSummon.Destroyer:
				NPC.SpawnOnPlayer(playerIndex, NPCID.TheDestroyer);
				break;

			case BossDomainSummon.Cultist:
				NPC.SpawnBoss((int)player.Center.X, (int)player.Center.Y - 120, NPCID.CultistBoss, playerIndex);
				break;
		}
	}
}
