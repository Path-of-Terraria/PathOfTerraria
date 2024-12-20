using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestSystem : ModSystem
{
	public static bool ForceGarrickSpawn()
	{
		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			return !NPC.downedSlimeKing && QuestUnlockManager.CanStartQuest<KingSlimeQuest>();
		}

		return false;
	}

	public override void ClearWorld()
	{
		if (Main.dedServ)
		{
			return;
		}

		// Hide UI as you switch worlds
		if (SmartUiLoader.GetUiState<QuestsUIState>().Visible)
		{
			SmartUiLoader.GetUiState<QuestsUIState>().Toggle();
		}
	}
}
