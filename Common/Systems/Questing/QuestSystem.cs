using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;

namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestSystem : ModSystem
{
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
