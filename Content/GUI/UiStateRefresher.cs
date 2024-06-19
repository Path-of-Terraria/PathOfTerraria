using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.ModPlayers;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

/// <summary>
/// This is used for recalculating the bounding box for the UI on resizes.
/// </summary>
internal class UiStateRefresher : ModSystem
{
	public override void Load()
	{
		Main.OnResolutionChanged += RefreshUi;
	}

	public override void Unload()
	{
		Main.OnResolutionChanged -= RefreshUi;
	}
	
	/// <summary>
	/// Forces the refresh a single time for when the world is loaded and the UI needs refreshing
	/// </summary>
	public override void OnWorldLoad(){
		UILoader.GetUIState<QuestPanelButton>().Refresh();
	}
	
	/// <summary>
	/// Refreshes the UI on resolution change.
	/// Need to call each of the UI states we need to refresh. Only needed for certain panels that have their bounding box smaller than the screen.
	/// </summary>
	/// <param name="newSize"></param>
	private static void RefreshUi(Vector2 newSize)
	{
		UILoader.GetUIState<QuestPanelButton>().Refresh();
	}
}