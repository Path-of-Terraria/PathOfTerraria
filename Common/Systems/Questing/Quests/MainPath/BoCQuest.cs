using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class BoCQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<LloydNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 7500, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ConditionCheck((_) => DisableEvilOrbBossSpawning.ActualOrbsSmashed > 0, 1, this.GetLocalization("SmashOrb")),
			new ConditionCheck((_) => SubworldSystem.Current is BrainDomain, 1, this.GetLocalization("EnterDomain")),
			//new InteractWithNPC(ModContent.NPCType<LloydNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.InDomain")),
			new KillCount(NPCID.BrainofCthulhu, 1, this.GetLocalization("KillBrain")),
			new InteractWithNPC(ModContent.NPCType<LloydNPC>(), Language.GetText("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.Complete"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}
}