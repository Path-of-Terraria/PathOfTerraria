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

internal class GolemQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<BlacksmithNPC>();

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
				new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.GolemDialogue1"), 
					Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.GolemDialogue1"),
					[
						new GiveItem(2, ItemID.LihzahrdPowerCell),
					]),
				//TODO: The below seems to not always count the Lihzahrds as kills and im not sure why. Something is weird here.
				new KillCount(npc => npc.netID == NPCID.Lihzahrd || npc.netID == NPCID.LihzahrdCrawler, 10, this.GetLocalization("Lihzards"))
			], Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.GolemDialogue1")),

			// TODO: REMOVE LATER for -> After step 1, Thrain will give you an item to use on the Altar in the temple. This will summon the Golem domain portal in the temple. 
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.GolemDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.GolemDialogue2"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<GolemMap>())),
			
			new ConditionCheck(_ => SubworldSystem.Current is DestroyerDomain, 1, this.GetLocalization("EnterDomain")),
			
			new ConditionCheck(_ => BossTracker.DownedInDomain<DestroyerDomain>(NPCID.TheDestroyer), 1, this.GetLocalization("Boss")),
	
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.GolemDialogue3"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.GolemDialogue3"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		Quest planteraQuest = GetLocalPlayerInstance<PlanteraQuest>();
		return planteraQuest.Completed && NPC.downedPlantBoss;
	}
}
