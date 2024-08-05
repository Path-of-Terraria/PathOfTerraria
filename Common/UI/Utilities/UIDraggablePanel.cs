using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

// ReSharper disable once InconsistentNaming
public class UIDraggablePanel : UIPanel
{
	// Stores the offset from the top left of the UIPanel while dragging.
	// ReSharper disable once InconsistentNaming
	private Vector2 _mouseOffset { get; set; }
	private bool Dragging { get; set; }

	private bool[] _scaling = [false, false];

	private readonly Vector2 _minSize = new(400f, 400f);

	private readonly bool _stopItemUse;

	private int _uiDelay = -1;

	private readonly UIPanel _header;
	private readonly bool _canResize = true;
	private readonly Dictionary<string, UIPanelTab> _menus;
	public string ActiveTab = "";
	public event Action OnActiveTabChanged;

	public bool Blocked = true;

	public UIDraggablePanel(bool stopItemUse, bool showCloseButton,
		IEnumerable<(string key, LocalizedText text)> menuOptions, int panelHeight, bool canResize)
	{
		_stopItemUse = stopItemUse;

		SetPadding(0);

		_header = new UIPanel();
		_header.SetPadding(0);
		_header.Width.Set(0, 1f);
		_header.Height.Set(panelHeight, 0f);
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

		_minSize.X = Math.Max(_minSize.X, left);
		_canResize = canResize;

		SetActivePage(_menus.Keys.First());
	}

	public override void RightMouseDown(UIMouseEvent evt)
	{
		if (Blocked || !_canResize)
		{
			return;
		}

		CalculatedStyle style = GetDimensions();

		float xGrabPoint = style.X + style.Width - evt.MousePosition.X;
		if (xGrabPoint > 0 && xGrabPoint < 30)
		{
			_scaling[0] = true;
		}

		float yGrabPoint = style.Y + style.Height - evt.MousePosition.Y;
		if (yGrabPoint > 0 && yGrabPoint < 30)
		{
			_scaling[1] = true;
		}

		base.RightMouseDown(evt);
	}

	public override void RightMouseUp(UIMouseEvent evt)
	{
		_scaling = [false, false];
		base.RightMouseUp(evt);
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
		
		ActiveTab = page;
		OnActiveTabChanged?.Invoke();
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

		if (!Blocked)
		{
			DragStart(evt);
		}
	}

	private void Header_MouseUp(UIMouseEvent evt, UIElement element)
	{
		base.LeftMouseUp(evt);

		Dragging = false;

		if (GetDimensions().Y < 0)
		{
			Top.Pixels -= GetDimensions().Y;
		}
	}

	private void DragStart(UIMouseEvent evt)
	{
		_mouseOffset = evt.MousePosition;
		Dragging = true;
	}

	public override void Update(GameTime gameTime)
	{
		if (_uiDelay > 0)
		{
			_uiDelay--;
		}

		// clicks on this UIElement dont cause the player to use current items. 
		if (ContainsPoint(Main.MouseScreen) && _stopItemUse)
		{
			Main.LocalPlayer.mouseInterface = true;
		}

		if (Dragging || _scaling[0] || _scaling[1])
		{
			CalculatedStyle style = GetDimensions();

			if (Dragging)
			{
				Vector2 difference = Main.MouseScreen - _mouseOffset;
				_mouseOffset = Main.MouseScreen;
				Left.Pixels += difference.X;
				Top.Pixels += difference.Y;
			}

			if (_scaling[0])
			{
				Width.Pixels = MathF.Max(Main.mouseX - style.X, _minSize.X);
			}

			if (_scaling[1])
			{
				Height.Pixels = MathF.Max(Main.mouseY - style.Y, _minSize.Y);
			}

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

		base.Update(gameTime); // don't remove.

		Blocked = Main.blockMouse;
	}
}