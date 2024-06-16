using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

public class UIDraggablePanel : UIPanel
{
	// Stores the offset from the top left of the UIPanel while dragging.
	private Vector2 Offset { get; set; }

	private bool Dragging { get; set; }

	private readonly bool _stopItemUse;

	private int _uiDelay = -1;

	private readonly UIPanel _header;

	private readonly Dictionary<string, UIPanelTab> _menus;

	public UIDraggablePanel(bool stopItemUse, bool showCloseButton,
		IEnumerable<(string key, LocalizedText text)> menuOptions)
	{
		_stopItemUse = stopItemUse;

		SetPadding(0);

		_header = new UIPanel();
		_header.SetPadding(0);
		_header.Width.Set(0, 1f);
		_header.Height.Set(32, 0f);
		_header.BackgroundColor.A = 255;
		_header.OnLeftMouseDown += Header_MouseDown;
		_header.OnLeftMouseUp += Header_MouseUp;
		Append(_header);

		if (showCloseButton)
		{
			var closeButton = new UITextPanel<char>('X');
			closeButton.SetPadding(7);
			closeButton.Width.Set(40, 0);
			closeButton.Left.Set(-40, 1);
			closeButton.BackgroundColor.A = 255;
			_header.Append(closeButton);
		}

		UIPanel viewArea = new();
		viewArea.Top.Set(38, 0);
		viewArea.Width.Set(0, 1f);
		viewArea.Height.Set(-38, 1f);
		viewArea.BackgroundColor = Color.Transparent;
		viewArea.BorderColor = Color.Transparent;
		Append(viewArea);

		_menus = new Dictionary<string, UIPanelTab>();

		float left = 0;

		foreach ((string key, LocalizedText text) in menuOptions)
		{
			UIPanelTab menu;
			_menus.Add(key, menu = new UIPanelTab(key, text));
			menu.SetPadding(7);
			menu.Left.Set(left, 0f);
			menu.BackgroundColor.A = 255;
			menu.Recalculate();
			menu.UseImmediateMode = true;
			menu.OnLeftClick += (evt, element) =>
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				SetActivePage(key);
			};

			left += menu.GetDimensions().Width + 10;

			_header.Append(menu);
		}
		
		SetActivePage(_menus.Keys.First());
	}

	public void SetActivePage(string page)
	{
		foreach (UIPanelTab tab in _menus.Values)
		{
			tab.TextColor = Color.White;
		}

		if (_menus.TryGetValue(page, out UIPanelTab menuText))
		{
			menuText.TextColor = Color.Yellow;
		}
	}

	public void HideTab(string page)
	{
		if (_menus.TryGetValue(page, out UIPanelTab tab))
		{
			tab.Remove();
			RecalculateTabPositions();
		}
	}

	public void ShowTab(string page)
	{
		if (_menus.TryGetValue(page, out UIPanelTab tab) && tab.Parent is null)
		{
			tab.Remove();
			_header.Append(tab);

			RecalculateTabPositions();
		}
	}

	private void RecalculateTabPositions()
	{
		float left = 0;

		foreach (UIPanelTab tab in _menus.Values)
		{
			if (tab.Parent is null)
			{
				continue;
			}

			tab.Left.Set(left, 0f);
			tab.Recalculate();

			left += tab.GetDimensions().Width + 10;
		}
	}

	private void Header_MouseDown(UIMouseEvent evt, UIElement element)
	{
		base.LeftMouseDown(evt);

		DragStart(evt);
	}

	private void Header_MouseUp(UIMouseEvent evt, UIElement element)
	{
		base.LeftMouseUp(evt);

		DragEnd(evt);
	}

	private void DragStart(UIMouseEvent evt)
	{
		Offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
		Dragging = true;
	}

	private void DragEnd(UIMouseEvent evt)
	{
		//A child element forced this to not move
		if (!Dragging)
		{
			return;
		}

		Vector2 end = evt.MousePosition;
		Dragging = false;

		Left.Set(end.X - Offset.X, 0f);
		Top.Set(end.Y - Offset.Y, 0f);

		Recalculate();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime); // don't remove.

		if (_uiDelay > 0)
		{
			_uiDelay--;
		}

		// clicks on this UIElement dont cause the player to use current items. 
		if (ContainsPoint(Main.MouseScreen) && _stopItemUse)
		{
			Main.LocalPlayer.mouseInterface = true;
		}

		if (Dragging)
		{
			Left.Set(Main.mouseX - Offset.X, 0f); // Main.MouseScreen.X and Main.mouseX are the same.
			Top.Set(Main.mouseY - Offset.Y, 0f);
			Recalculate();
		}

		// Here we check if the UIDragablePanel is outside the Parent UIElement rectangle. 
		// By doing this and some simple math, we can snap the panel back on screen if the user resizes his window or otherwise changes resolution.
		var parentSpace = Parent.GetDimensions().ToRectangle();

		if (!GetDimensions().ToRectangle().Intersects(parentSpace))
		{
			// TODO: account for negative Pixels and > 0 Percent
			Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
			Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);

			// Recalculate forces the UI system to do the positioning math again.
			Recalculate();
		}
	}
}