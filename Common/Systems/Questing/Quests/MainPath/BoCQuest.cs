using PathOfTerraria.Common.Quests;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
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
			new ConditionCheck("SmashOrbs", (_) => DisableEvilOrbBossSpawning.ActualOrbsSmashed > 0, 1, this.GetLocalization("SmashOrb")),
			new ConditionCheck("EnterDomain", (_) => SubworldSystem.Current is BrainDomain, 1, this.GetLocalization("EnterDomain")),
			new ConditionCheck("KillBrain", _ => BossTracker.DownedInDomain<BrainDomain>(NPCID.BrainofCthulhu), 1, this.GetLocalization("KillBrain"))
			{
				SkipCheck = QuestUtils.BossSkipCheck(NPCID.BrainofCthulhu)
			},
			new InteractWithNPC("TalkLloyd", ModContent.NPCType<LloydNPC>(), LocalizedText.Empty, Language.GetText("Mods.PathOfTerraria.NPCs.LloydNPC.Dialogue.Complete"))
			{
				CountsAsCompletedOnMarker = true
			},
		];
	}

	protected override bool InternalAvailable()
	{
		return NPC.downedBoss1;
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}
}