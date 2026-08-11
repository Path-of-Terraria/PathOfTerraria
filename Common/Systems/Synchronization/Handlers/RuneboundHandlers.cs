using System.IO;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;
using PathOfTerraria.Common.Systems.Runebound;
using PathOfTerraria.Content.NPCs.Runebound;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal sealed class RuneboundTutorialRequestHandler : Handler
{
	public static void Send()
	{
		Networking.GetPacket<RuneboundTutorialRequestHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		if (sender >= Main.maxPlayers)
		{
			return;
		}

		Player player = Main.player[sender];

		if (player.active && Quest.PlayerHasQuest<TheFirstBindingQuest>(sender))
		{
			player.GetModPlayer<RuneboundPlayer>().TutorialEncounterRequested = true;
		}
	}
}

internal sealed class RuneboundActivateHandler : Handler
{
	public static void Send(short npcIndex)
	{
		ModPacket packet = Networking.GetPacket<RuneboundActivateHandler>();
		packet.Write(npcIndex);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		short npcIndex = reader.ReadInt16();

		if (sender >= Main.maxPlayers || npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active
			|| Main.npc[npcIndex].ModNPC is not BindingMonolith monolith)
		{
			return;
		}

		Player player = Main.player[sender];

		if (player.active && player.DistanceSQ(monolith.NPC.Center) <= 320f * 320f)
		{
			monolith.Activate(player);
		}
	}
}

internal sealed class RuneboundProgressHandler : Handler
{
	public static void Send(Player player)
	{
		RuneboundPlayer progress = player.GetModPlayer<RuneboundPlayer>();
		ModPacket packet = Networking.GetPacket<RuneboundProgressHandler>();
		packet.Write(progress.TutorialEncounterCompleted);
		packet.Write(progress.EncountersCompleted);
		packet.Send(player.whoAmI);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		RuneboundPlayer progress = Main.LocalPlayer.GetModPlayer<RuneboundPlayer>();
		progress.TutorialEncounterCompleted = reader.ReadBoolean();
		if (progress.TutorialEncounterCompleted)
		{
			progress.TutorialEncounterRequested = false;
		}
		progress.EncountersCompleted = reader.ReadInt32();
	}
}

internal sealed class RuneboundStateHandler : Handler
{
	public static void Send(RuneboundPlayer progress)
	{
		ModPacket packet = Networking.GetPacket<RuneboundStateHandler>();
		packet.Write(progress.TutorialEncounterCompleted);
		packet.Write(progress.HasCraftedRunestone);
		packet.Write(progress.LearnedMechanic);
		packet.Write(progress.EncountersCompleted);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		bool tutorialCompleted = reader.ReadBoolean();
		bool hasCrafted = reader.ReadBoolean();
		bool learned = reader.ReadBoolean();
		int encountersCompleted = reader.ReadInt32();
		if (sender >= Main.maxPlayers)
		{
			return;
		}

		Player player = Main.player[sender];

		if (!player.active)
		{
			return;
		}

		RuneboundPlayer progress = player.GetModPlayer<RuneboundPlayer>();
		progress.TutorialEncounterCompleted |= tutorialCompleted;
		progress.HasCraftedRunestone |= hasCrafted;
		progress.LearnedMechanic |= learned && progress.TutorialEncounterCompleted && progress.HasCraftedRunestone;
		progress.EncountersCompleted = Math.Max(progress.EncountersCompleted, Math.Max(0, encountersCompleted));
	}
}
