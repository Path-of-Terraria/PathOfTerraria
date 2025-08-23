using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath.HardmodeQuesting;

internal class DestroyerQuest() : HardmodeQuest(3)
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<BlacksmithNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) =>
		{
			p.GetModPlayer<ExpModPlayer>().Exp += 30000;
			p.GetModPlayer<QuestModPlayer>().StartQuest<SkelePrimeQuest>();
		},
			"30000 experience"),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ConditionCheck(_ => 
			{
				MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;
				return tracker.CompletionsAtOrAboveTier(3) >= MappingDomainSystem.RequiredCompletionsPerTier;
			}, 1, () => this.GetLocalization("Tiers").WithFormatArgs(
				MathHelper.Clamp(ModContent.GetInstance<MappingDomainSystem>().Tracker.CompletionsAtOrAboveTier(QuestTier), 0, MappingDomainSystem.RequiredCompletionsPerTier),
				MappingDomainSystem.RequiredCompletionsPerTier
			)),
			new ConditionCheck(_ => SubworldSystem.Current is DestroyerDomain, 1, this.GetLocalization("EnterDomain")),
			new ConditionCheck(_ => BossTracker.DownedInDomain<DestroyerDomain>(NPCID.TheDestroyer), 1, this.GetLocalization("Boss")),
		];
	}

	public override string MarkerLocation()
	{
		return "Overworld";
	}

	public override bool Available()
	{
		return false;
	}
}
