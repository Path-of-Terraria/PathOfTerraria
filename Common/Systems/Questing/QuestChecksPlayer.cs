using System.Linq;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestChecksPlayer : ModPlayer
{
	private static Quest[] ClassQuests => [ModContent.GetInstance<BlacksmithStartQuest>(), ModContent.GetInstance<WizardStartQuest>(),
		ModContent.GetInstance<WitchStartQuest>(), ModContent.GetInstance<HunterStartQuest>()];

	public bool CanWoFQuest { get; private set; }
	public bool CanKingSlimeQuest { get; private set; }

	public override void ResetEffects()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			if (!CanKingSlimeQuest)
			{
				CanKingSlimeQuest = ClassQuests.Count(x => x.Completed) >= 3;
			}

			if (!CanWoFQuest)
			{
				CanWoFQuest = NPC.downedDeerclops.ToInt() + NPC.downedQueenBee.ToInt() + NPC.downedBoss3.ToInt()
					+ NPC.downedBoss2.ToInt() >= 3;
			}
		}
	}
}
