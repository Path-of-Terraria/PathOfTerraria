namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

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

internal class TinkerIntroQuest() : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct2;
	public override int NPCQuestGiver => ModContent.NPCType<TinkerNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 30000;
		}, "30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue1"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue1"), 
				[
					new GiveItem(100, ItemID.Wood),
					new GiveItem(20, ItemID.Wire),
					new GiveItem(50, ItemID.StoneBlock),
					new GiveItem(1, ItemID.Teleporter),
				], true), 
			
			//TODO Add the construction of her workshop here.
			
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue2"), 
				Language.GetText("Mods.PathOfTerraria.NPCs.TinkerNPC.Dialogue.TinkerIntroDialogue2")),
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		return Main.hardMode;
	}
}
