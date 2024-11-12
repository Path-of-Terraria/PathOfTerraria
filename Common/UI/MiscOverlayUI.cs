using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

/// <summary>
/// Handles misc overlay UIs.
/// </summary>
internal class MiscOverlayUI : SmartUiState
{
	/// <summary>
	/// Draws under all other UI but over all world visuals. Scales by <see cref="InterfaceScaleType.Game"/> scale, not by <see cref="InterfaceScaleType.UI"/> scale.
	/// </summary>
	public static event Action<SpriteBatch> DrawOverlay;

	public override void Unload()
	{
		DrawOverlay = null;
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(x => x.Name == "Vanilla: MP Player Names");
	}

	public override void OnInitialize()
	{
		Visible = true;
		Scale = InterfaceScaleType.Game;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		DrawOverlay?.Invoke(spriteBatch);
	}
}
