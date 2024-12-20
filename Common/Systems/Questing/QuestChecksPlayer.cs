using System.Linq;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestChecksPlayer : ModPlayer
{
	private static Quest[] ClassQuests => [Quest.GetLocalPlayerInstance<BlacksmithStartQuest>(),
		Quest.GetLocalPlayerInstance<WizardStartQuest>(),
		Quest.GetLocalPlayerInstance<WitchStartQuest>(),
		Quest.GetLocalPlayerInstance<HunterStartQuest>()];

	public bool CanKingSlimeQuest { get; private set; }

	public override void ResetEffects()
	{
		if (!Main.gameMenu && Main.myPlayer == Player.whoAmI && Player.TryGetModPlayer(out QuestModPlayer _))
		{
			if (!CanKingSlimeQuest)
			{
				CanKingSlimeQuest = ClassQuests.Count(x => x.Completed) >= 3;
			}
		}
	}
}
