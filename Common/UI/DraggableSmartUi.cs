using System.Collections.Generic;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

public abstract class DraggableSmartUi : SmartUiState
{
	public UIDraggablePanel Panel;
	public UIImageButton CloseButton;
	public bool IsVisible;

	protected const int PointsAndExitPadding = 10;

	protected const int DraggablePanelHeight = 32;

	protected const int TopPadding = -400;
	protected const int PanelHeight = 800 - DraggablePanelHeight;
	protected const int LeftPadding = -450;
	protected const int PanelWidth = 900;
	
	public override bool Visible => IsVisible;
	
	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	protected virtual void CreateMainPanel((string key, LocalizedText text)[] tabs, bool showCloseButton = true, Point? panelSize = null, bool canResize = true)
	{
		panelSize ??= new Point(PanelWidth, PanelHeight);

		Panel = new UIDraggablePanel(false, false, tabs, DraggablePanelHeight, canResize);
		Panel.Left.Set(LeftPadding, 0.5f);
		Panel.Top.Set(TopPadding, 0.5f);
		Panel.Width.Set(panelSize.Value.X, 0);
		Panel.Height.Set(panelSize.Value.Y, 0);
		Append(Panel);

		if (showCloseButton)
		{
			AddCloseButton();	
		}
	}
	
	protected void AddCloseButton()
	{
		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"));
		CloseButton.Left.Set(-38 - PointsAndExitPadding, 1f);
		CloseButton.Top.Set(10, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (_, _) => DefaultClose();
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);
	}

	protected virtual void DefaultClose()
	{
		IsVisible = false;
		SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
	}
}