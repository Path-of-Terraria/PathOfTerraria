using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class MigrateRavencrestUI : UIState
{
	public override void OnInitialize()
	{
		UIPanel panel = new()
		{
			VAlign = 0.25f,
			HAlign = 0.5f,
			Width = StyleDimension.FromPixels(600),
			Height = StyleDimension.FromPixels(80),
		};
		Append(panel);

		UIText migrateText = new("Migrate Ravencrest?")
		{
			VAlign = -1,
		};
		panel.Append(migrateText);

		UIText desc = new("Ravencrest is outdated, and may have issues. Do you want to update your save?", 0.9f);
		panel.Append(desc);

		UIButton<string> yesButton = new("Yes")
		{
			Width = StyleDimension.FromPixels(80),
			Height = StyleDimension.FromPixels(36),
			VAlign = 1
		};
		panel.Append(yesButton);

		UIButton<string> noButton = new("No")
		{
			Width = StyleDimension.FromPixels(80),
			Height = StyleDimension.FromPixels(36),
			Left = StyleDimension.FromPixels(84),
			VAlign = 1
		};
		panel.Append(noButton);
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();
	}
}
