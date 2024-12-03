﻿using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;

namespace PathOfTerraria.Common.Systems.Questing;

internal class QuestSystem : ModSystem
{
	public override void ClearWorld()
	{
		// Reset quests on exit so we don't bleed data
		foreach (Quest quest in ModContent.GetContent<Quest>())
		{
			quest.Active = false;
			quest.Completed = false;
			quest.CurrentStep = 0;
		}

		// Hide UI as you switch worlds
		if (SmartUiLoader.GetUiState<QuestsUIState>().Visible)
		{
			SmartUiLoader.GetUiState<QuestsUIState>().Toggle();
		}
	}
}
