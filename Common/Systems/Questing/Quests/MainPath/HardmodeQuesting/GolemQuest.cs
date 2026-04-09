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
			new KillCount("Kill", npc => npc.netID == NPCID.Lihzahrd || npc.netID == NPCID.LihzahrdCrawler, 10, this.GetLocalization("Lihzards")),

			new InteractWithNPC("GiveCells", NPCQuestGiver, LocalizedText.Empty,
				Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.Golem.0"),
				[
					new GiveItem(2, ItemID.LihzahrdPowerCell),
				]),

			// TODO: REMOVE LATER for -> After step 1, Thrain will give you an item to use on the Altar in the temple. This will summon the Golem domain portal in the temple. 
			new InteractWithNPC("Talk", NPCQuestGiver, this.GetLocalization("Lihzards"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.Golem.1"),
				onSuccess: _ => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<GolemMap>())),
			
			new ConditionCheck("Domain", _ => SubworldSystem.Current is GolemDomain, 1, this.GetLocalization("EnterDomain")),
			
			new ConditionCheck("Boss", _ => BossTracker.DownedInDomain<GolemDomain>(NPCID.Golem), 1, this.GetLocalization("Boss")),
	
			new InteractWithNPC("Finish", NPCQuestGiver, this.GetLocalization("Boss"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.Golem.2"))
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	protected override bool InternalAvailable()
	{
		Quest planteraQuest = GetLocalPlayerInstance<PlanteraQuest>();
		return planteraQuest.Completed && NPC.downedPlantBoss;
	}
}
