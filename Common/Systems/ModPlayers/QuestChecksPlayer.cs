using System.Linq;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class QuestChecksPlayer : ModPlayer
{
	public bool CanWoFQuest { get; private set; }

	private Quest[] WoFQuests => [ModContent.GetInstance<QueenBeeQuest>(), ModContent.GetInstance<BoCQuest>(), ModContent.GetInstance<EoWQuest>(),
		ModContent.GetInstance<DeerclopsQuest>()];

	public override void ResetEffects()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			CanWoFQuest = WoFQuests.Count(x => x.Completed) >= 3;
		}
	}
}
