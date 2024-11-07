using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

internal class MiscOverlayUI : SmartUiState
{
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
