using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.NPCs.Town;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class TinkerIntroQuest() : Quest
{
	public static uint? CompletionVisit { get; private set; }

	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<TinkerNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 30000;
			CompletionVisit = ModContent.GetInstance<RavencrestSubworld>().TimesEntered;
		}, "30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC("Start", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue1"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue1"), 
				[
					new GiveItem(100, ItemID.Wood),
					new GiveItem(20, ItemID.Wire),
					new GiveItem(50, ItemID.StoneBlock),
					new GiveItem(1, ItemID.Teleporter),
				], true), 
			
			new ActionStep((_, _) => 
			{
				RavencrestSystem.UpgradeBuilding("Workshop");
				return true;
			}),
			
			new InteractWithNPC("Finish", NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue2")),
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	protected override bool InternalAvailable()
	{
		return Main.hardMode;
	}
}
