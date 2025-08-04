using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class KingSlimeQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => ModContent.NPCType<GarrickNPC>();

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 500,
			"500 experience (POC giving experience)\nSome gear with an affix\nA unique item\nAgain, just for POC reasons"),
	];

	public override List<QuestStep> SetSteps()
	{
		return 
		[
			new ConditionCheck(_ => SubworldSystem.Current is KingSlimeDomain, 1, this.GetLocalization("EnterDomain")),
			new ConditionCheck(_ => NPC.downedSlimeKing, 1, this.GetLocalization("Kill.KingSlime")),
			new InteractWithNPC(ModContent.NPCType<GarrickNPC>(), this.GetLocalization("ThanksDialogue")) { CountsAsCompletedOnMarker = true }
		];
	}

	public override bool Available()
	{
		Quest[] checks = 
		[
			GetLocalPlayerInstance<BlacksmithStartQuest>(),
			GetLocalPlayerInstance<WizardStartQuest>(),
			GetLocalPlayerInstance<WitchStartQuest>(),
			GetLocalPlayerInstance<HunterStartQuest>()
		];

		return checks.Any(x => x.Completed);
	}

	public override string MarkerLocation()
	{
		return "Ravencrest";
	}
}