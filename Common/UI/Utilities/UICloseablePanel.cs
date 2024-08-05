using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

// ReSharper disable once InconsistentNaming
public class UICloseablePanel : UIPanel
{
	private bool[] _scaling = [false, false];

	private readonly Vector2 _minSize = new(400f, 400f);

	private readonly bool _stopItemUse;

	private int _uiDelay = -1;

	private readonly UIPanel _header;
	private readonly bool _canResize = true;

	public bool Blocked = true;
	public bool Visible = true;

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (!Visible)
		{
			return;
		}

		base.DrawSelf(spriteBatch);
#if DEBUG
		GUIDebuggingTools.DrawGuiBorder(spriteBatch, this, Color.Blue);
#endif
	}

	public UICloseablePanel(bool stopItemUse, bool showCloseButton, bool canResize, bool invisible)
	{
		_stopItemUse = stopItemUse;

		SetPadding(0);

		_header = new UIPanel();
		_header.SetPadding(0);
		_header.Width.Set(0, 0f);
		_header.Height.Set(0, 0f);
		_header.BackgroundColor.A = 255;

		if (!invisible)
		{
			Append(_header);
		}

		if (showCloseButton)
		{
			var closeButton = new UITextPanel<char>('X');
			closeButton.SetPadding(7);
			closeButton.Width.Set(40, 0);
			closeButton.Left.Set(-40, 1);
			closeButton.BackgroundColor.A = 255;
			_header.Append(closeButton);
		}

		float left = 0;

		_minSize.X = Math.Max(_minSize.X, left);
		_canResize = canResize;
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