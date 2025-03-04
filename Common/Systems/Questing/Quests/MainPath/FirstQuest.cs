using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Questing.QuestStepTypes;
using PathOfTerraria.Common.Systems.Questing.RewardTypes;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

internal class FirstQuest : Quest
{
	public override QuestTypes QuestType => QuestTypes.MainStoryQuestAct1;
	public override int NPCQuestGiver => -1;

	public override List<QuestReward> QuestRewards =>
	[
		new ActionRewards((p, v) => p.GetModPlayer<ExpModPlayer>().Exp += 500, ""),
	];

	public override List<QuestStep> SetSteps()
	{
		return
		[
			new ConditionCheck(plr => plr.DistanceSQ(ModContent.GetInstance<RavencrestSystem>().EntrancePosition.ToWorldCoordinates()) < 400 * 400,
				1, this.GetLocalization("ApproachEntrance")),
			new ConditionCheck(_ => SubworldSystem.Current is RavencrestSubworld, 1, this.GetLocalization("EnterRavencrest")),
		];
	}

	public override bool Available()
	{
		return true;
	}

	public override string MarkerLocation()
	{
		return "";
	}
}