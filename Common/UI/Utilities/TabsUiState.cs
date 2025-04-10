using System.Collections.Generic;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

public abstract class TabsUiState : CloseableSmartUi
{
	/// <summary>
	/// Shorthand for <c>Panel as UITabsPanel</c>.
	/// </summary>
	public UITabsPanel TabPanel => Panel as UITabsPanel;

	protected override int PointsAndExitPadding => 10;

	protected const int DraggablePanelHeight = 32;

	protected override int TopPadding => -400;
	protected override int PanelHeight => 800 - DraggablePanelHeight;
	protected override int LeftPadding => -450;
	protected override int PanelWidth => 900;
	
	public override bool Visible => IsVisible;
	public virtual List<SmartUiElement> TabPanels => [];
	
	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	protected virtual void CreateMainPanel((string key, LocalizedText text)[] tabs, bool showCloseButton = true, Point? panelSize = null, Action extraOnTabChanged = null)
	{
		panelSize ??= new Point(PanelWidth, PanelHeight);

		Panel = new UITabsPanel(false, false, tabs, DraggablePanelHeight);
		TabPanel.OnActiveTabChanged += HandleActiveTabChanged;

		if (extraOnTabChanged is not null)
		{
			TabPanel.OnActiveTabChanged += extraOnTabChanged;
		}

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

	public virtual void AppendChildren()
	{
		foreach (SmartUiElement tabPanel in TabPanels)
		{
			tabPanel.Left.Set(0, 0);
			tabPanel.Top.Set(DraggablePanelHeight, 0);
			tabPanel.Width.Set(0, 1f);
			tabPanel.Height.Set(-DraggablePanelHeight, 1f);
		}

		HandleActiveTabChanged();
	}

	private void HandleActiveTabChanged()
	{
		foreach (SmartUiElement tabPanel in TabPanels)
		{
			if (tabPanel.TabName != TabPanel.ActiveTab)
			{
				tabPanel.Remove();
				continue;
			}
			
			Panel.Append(tabPanel);
		}
	}
	
	public Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}