using System.Collections.Generic;
using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Core.Loaders.UILoading;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

public abstract class DraggableSmartUi : SmartUIState
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
	public virtual List<SmartUIElement> TabPanels => [];
	
	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	protected virtual void CreateMainPanel((string key, LocalizedText text)[] tabs, bool showCloseButton = true)
	{
		Panel = new UIDraggablePanel(false, false, tabs, DraggablePanelHeight);
		Panel.OnActiveTabChanged += HandleActiveTabChanged;
		Panel.Left.Set(LeftPadding, 0.5f);
		Panel.Top.Set(TopPadding, 0.5f);
		Panel.Width.Set(PanelWidth, 0);
		Panel.Height.Set(PanelHeight, 0);
		Append(Panel);
		if (showCloseButton)
		{
			AddCloseButton();	
		}
	}
	
	protected void AddCloseButton()
	{
		CloseButton =
			new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
		CloseButton.Left.Set(-38 - PointsAndExitPadding, 1f);
		CloseButton.Top.Set(10, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) =>
		{
			IsVisible = false;
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);
	}

	public virtual void AppendChildren()
	{
		foreach (SmartUIElement tabPanel in TabPanels)
		{
			tabPanel.Left.Set(0, 0);
			tabPanel.Top.Set(DraggablePanelHeight, 0);
			tabPanel.Width.Set(0, 1f);
			tabPanel.Height.Set(-DraggablePanelHeight, 1f);
			Panel.Append(tabPanel);
		}
	}

	private void HandleActiveTabChanged()
	{
		foreach (SmartUIElement tabPanel in TabPanels)
		{
			if (tabPanel.TabName != Panel.ActiveTab)
			{
				tabPanel.Remove();
				break;
			}
			
			Panel.Append(tabPanel);
		}
	}
}