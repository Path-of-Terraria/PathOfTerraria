using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.Items.Quest;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class SkeletronQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => NPCID.OldMan;

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 10000, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new InteractWithNPC(ModContent.NPCType<BlacksmithNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.BlacksmithNPC.Dialogue.OldManDialogue")),
			new InteractWithNPC(NPCQuestGiver, Language.GetText("Mods.PathOfTerraria.NPCs.OldMan.Dialogue.HasItems"), 
				[new GiveItem(2, ItemID.Candle, ItemID.PlatinumCandle), new GiveItem(1, ItemID.CrimtaneBar, ItemID.DemoniteBar),
					new GiveItem(1, ModContent.ItemType<AncientEvilBook>())]),
			new ConditionCheck((_) => SubworldSystem.Current is SkeletronDomain, 1, this.GetLocalization("EnterDomain")),
			new KillCount(NPCID.SkeletronHead, 1, this.GetLocalization("KillSkeletron")),
			new InteractWithNPC(NPCID.Clothier, Language.GetText("Mods.PathOfTerraria.NPCs.OldMan.Dialogue.Complete"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}
}