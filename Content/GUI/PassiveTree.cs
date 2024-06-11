using Microsoft.Xna.Framework.Graphics;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Content.GUI;

internal class PassiveTree : SmartUIState
{
	public UIPanel Panel;
	public UIImageButton CloseButton;
	public InnerPanel Inner;

	public bool IsVisible;

	protected static TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

	protected const int TopPadding = -400;
	protected const int LeftPadding = -450;
	protected const int PanelWidth = 800;
	protected const int PanelHeight = 750;

	public override bool Visible => IsVisible;
	private PlayerClass _currentDisplay = PlayerClass.None;
	public void Toggle(PlayerClass newClass = PlayerClass.None)
	{
		if (newClass == PlayerClass.None || IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (_currentDisplay != newClass)
		{
			_currentDisplay = newClass;
			RemoveAllChildren();
			DrawPanel();
			DrawCloseButton();
			DrawInnerPanel();

			TreeSystem.Nodes
				.Where(x => x.Classes.Contains(_currentDisplay))
				.ToList()
				.ForEach(n => Inner.Append(new PassiveElement(n)));
		}

		IsVisible = true;
	}
	public override void Draw(SpriteBatch spriteBatch)
	{
		if (Main.LocalPlayer.controlInv)
		{
			IsVisible = false;
		}

		Recalculate();
		base.Draw(spriteBatch);
		DrawPanelText(spriteBatch);
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	protected void DrawPanel()
	{
		Panel = new UIPanel();
		Panel.Left.Set(LeftPadding, 0.5f);
		Panel.Top.Set(TopPadding, 0.5f);
		Panel.Width.Set(PanelWidth, 0.15f);
		Panel.Height.Set(PanelHeight, 0.15f);
		Append(Panel);
	}

	protected void DrawCloseButton()
	{
		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/CloseButton"));
		CloseButton.Left.Set(LeftPadding + PanelWidth - 120, 0.5f);
		CloseButton.Top.Set(TopPadding + 10, 0.5f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) => {
			IsVisible = false;
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		CloseButton.SetVisibility(1, 1);
		Append(CloseButton);
	}
	
	protected void DrawInnerPanel()
	{
		Inner = new InnerPanel();
		Inner.Left.Set(0, 0);
		Inner.Top.Set(0, 0);
		Inner.Width.Set(PanelWidth - 0, 0);
		Inner.Height.Set(PanelHeight - 0, 0);
		Panel.Append(Inner);
	}

	protected void DrawPanelText(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/PassiveFrameSmall").Value;
		TreePlayer mp = Main.LocalPlayer.GetModPlayer<TreePlayer>();

		spriteBatch.Draw(tex, GetRectangle().TopLeft() + new Vector2(32, 32), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
		Utils.DrawBorderStringBig(spriteBatch, $"{mp.Points}", GetRectangle().TopLeft() + new Vector2(32, 32), mp.Points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
		Utils.DrawBorderStringBig(spriteBatch, $"Points remaining", GetRectangle().TopLeft() + new Vector2(170, 32), Color.White, 0.6f, 0.5f, 0.35f);
	}

	public Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}

internal class InnerPanel : SmartUIElement
{
	private Vector2 _start;
	private Vector2 _root;
	private Vector2 _lineOff;
	private Rectangle _innerContainer;
	private Point _containerSize;

	private UIElement Panel => Parent;

	private TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

	public override void Recalculate()
	{
		Vector2 min = new(float.MaxValue);
		Vector2 max = new(float.MinValue);

		foreach (UIElement item in Children)
		{
			var rect = item.GetDimensions().ToRectangle();

			if (item is PassiveElement pass)
			{
				rect.Width = 50;
				rect.Height = 50;
				rect.Inflate(-20, -20);
			}

			if (rect.Left < min.X)
			{
				min.X = rect.X;
			}

			if (rect.Right > max.X)
			{
				max.X = rect.Right;
			}

			if (rect.Top < min.Y)
			{
				min.Y = rect.Top;
			}

			if (rect.Bottom > max.Y)
			{
				max.Y = rect.Bottom;
			}
		}

		_containerSize = new Point((int)(max.X - min.X), (int)(max.Y - min.Y));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
		spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
		spriteBatch.GraphicsDevice.ScissorRectangle = Panel.GetDimensions().ToRectangle();

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

		foreach (PassiveEdge edge in TreeSystem.Edges)
		{
			Texture2D chainTex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Link").Value;

			Color color = Color.Gray;

			if (edge.End.CanAllocate(Main.LocalPlayer) && edge.Start.Level > 0)
			{
				color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
			}

			if (edge.End.Level > 0 && edge.Start.Level > 0)
			{
				color = Color.White;
			}

			for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(edge.Start.TreePos, edge.End.TreePos) / 16))
			{
				Vector2 pos = GetDimensions().Position() + Vector2.Lerp(edge.Start.TreePos, edge.End.TreePos, k) + _lineOff;
				Main.spriteBatch.Draw(chainTex, pos, null, color, edge.Start.TreePos.DirectionTo(edge.End.TreePos).ToRotation(), chainTex.Size() / 2, 1, 0, 0);
			}

			if (edge.End.Level > 0 && edge.Start.Level > 0)
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GlowAlpha").Value;
				var glowColor = new Color(255, 230, 150)
				{
					A = 0
				};

				var rand = new Random(edge.GetHashCode());

				for (int k = 0; k < 8; k++)
				{
					float dist = Vector2.Distance(edge.Start.TreePos, edge.End.TreePos);
					float len = (40 + rand.Next(120)) * dist / 50;
					float scale = 0.05f + rand.NextSingle() * 0.15f;

					float progress = (Main.GameUpdateCount + 15 * k) % len / (float)len;
					Vector2 pos = GetDimensions().Position() + Vector2.SmoothStep(edge.Start.TreePos, edge.End.TreePos, progress) + _lineOff;
					float scale2 = (float)Math.Sin(progress * 3.14f) * (0.4f - scale);
					spriteBatch.Draw(glow, pos, null, glowColor * scale2, 0, glow.Size() / 2f, scale2, 0, 0);
				}
			}
		}

		base.Draw(spriteBatch);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

		spriteBatch.GraphicsDevice.ScissorRectangle = oldRect;
		spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
	}

	private bool _blockMouse = false;
	private bool _isHovering = false;
	private bool _lastState = false;
	
	public override void SafeUpdate(GameTime gameTime)
	{
		if (Main.mouseLeft && !_lastState)
		{
			_blockMouse = GetDimensions().ToRectangle().Contains(Main.mouseX, Main.mouseY);
			_isHovering = Panel.IsMouseHovering;
		}
		else if (!Main.mouseLeft)
		{
			_blockMouse = _isHovering = false;
		}

		if (_isHovering)
		{
			if (_start == Vector2.Zero)
			{
				_start = Main.MouseScreen;
				_root = _lineOff;

				foreach (UIElement element in Elements)
				{
					if (element is PassiveElement ele)
					{
						ele.Root = new Vector2(ele.Left.Pixels, ele.Top.Pixels);
					}
				}
			}

			Vector2 containerPos = _root + Main.MouseScreen - _start;
			_innerContainer = new Rectangle((int)containerPos.X + _containerSize.X / 2, (int)containerPos.Y + _containerSize.Y / 2 + 40, _containerSize.X, _containerSize.Y);

			// Use this if you want to visualize the container hitbox.
			// Dust.QuickBox(_innerContainer.TopLeft() + Main.screenPosition, _innerContainer.BottomRight() + Main.screenPosition, 20, Color.White, null);

			if (_innerContainer.Intersects(Parent.GetDimensions().ToRectangle()))
			{
				foreach (UIElement element in Elements)
				{
					if (element is PassiveElement ele)
					{
						Vector2 position = ele.Root + Main.MouseScreen - _start;

						element.Left.Set(position.X, 0);
						element.Top.Set(position.Y, 0);
					}
				}

				_lineOff = _root + Main.MouseScreen - _start;
			}
		}
		else
		{
			_start = Vector2.Zero;
		}

		Main.blockMouse = _blockMouse;
		_lastState = Main.mouseLeft;

		Recalculate();
	}
}