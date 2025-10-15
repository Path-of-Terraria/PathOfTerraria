using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class FishronQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<FishermanNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
			{
				p.GetModPlayer<ExpModPlayer>().Exp += 30000;
			},
			"30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ParallelQuestStep([
				new CollectCount(ItemID.Coral, 10),
				//TODO: Change to be 5 of ANY fish.
				new CollectCount(ItemID.Fish, 5),
				new KillCount(npc => npc.netID == NPCID.Shark || npc.netID == NPCID.Crab || npc.netID == NPCID.BlueJellyfish || npc.netID == NPCID.GreenJellyfish || npc.netID == NPCID.PinkJellyfish, 10, this.GetLocalization("OceanEnemies"))
			], Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue1")),

			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue2")),
			
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue3"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue3"),
				[
					new GiveItem(1, ItemID.TruffleWorm),
				],
				//TODO: This needs to be changed to the rift in the ocean, throwing a truffle worm in it will drag you into it, teleporting to the fishrom map.
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<FishronMap>())),

			new ConditionCheck(_ => SubworldSystem.Current is FishronDomain, 1, this.GetLocalization("EnterDomain")),
			
			new ConditionCheck(_ => BossTracker.DownedInDomain<FishronDomain>(NPCID.DukeFishron), 1, this.GetLocalization("Boss")),
	
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue4"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.FishermanNPC.Dialogue.FishermanFishronDialogue4"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest golemQuest = GetLocalPlayerInstance<GolemQuest>();
		return golemQuest.Completed && NPC.downedGolemBoss;
	}
}
