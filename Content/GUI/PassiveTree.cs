using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class PassiveTree : SmartUIState
{
	public UIPanel Panel;
	public UIImageButton CloseButton;
	public InnerPanel Inner;

	public bool IsVisible;

	protected static TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

	protected const int TopPadding = -400;
	protected const int PanelHeight = 800;
	protected const int LeftPadding = -450;
	protected const int PanelWidth = 900;

	public Vector2 TopLeftTree;
	public Vector2 BotRightTree;

	public override bool Visible => IsVisible;
	public PlayerClass CurrentDisplayClass = PlayerClass.None;

	public void Toggle(PlayerClass newClass = PlayerClass.None)
	{
		if (newClass == PlayerClass.None || IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (CurrentDisplayClass != newClass)
		{
			TopLeftTree = Vector2.Zero;
			BotRightTree = Vector2.Zero;
			CurrentDisplayClass = newClass;
			RemoveAllChildren();
			DrawPanel();
			DrawInnerPanel();
			DrawCloseButton();

			TreeSystem.CreateTree();
			if (Inner != null)
			{
				TreeSystem.ActiveNodes.ForEach(n => Inner.Append(new PassiveElement(n)));
			}
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
		var localizedTexts = new (string key, LocalizedText text)[]
		{
			("PassiveTree", Language.GetText("Mods.PathOfTerraria.GUI.PassiveTreeTab")),
			("SkillTree", Language.GetText("Mods.PathOfTerraria.GUI.SkillTreeTab"))
		};
		Panel = new UIDraggablePanel(false, false, localizedTexts);
		Panel.Left.Set(LeftPadding, 0.5f);
		Panel.Top.Set(TopPadding, 0.5f);
		Panel.Width.Set(PanelWidth, 0);
		Panel.Height.Set(PanelHeight, 0);
		Append(Panel);
	}
	
	protected void DrawInnerPanel()
	{
		Inner = new InnerPanel();
		Inner.Left.Set(0, 0);
		Inner.Top.Set(32, 0);
		Inner.Width.Set(0, 2f);
		Inner.Height.Set(0, 2f);
		Panel.Append(Inner);
	}

	protected void DrawCloseButton()
	{
		CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/CloseButton"));
		CloseButton.Left.Set(-38, 1f);
		CloseButton.Top.Set(0, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) => {
			IsVisible = false;
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);
	}

	protected void DrawPanelText(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/PassiveFrameSmall").Value;
		TreePlayer mp = Main.LocalPlayer.GetModPlayer<TreePlayer>();

		spriteBatch.Draw(tex, GetRectangle().TopLeft() + new Vector2(32, 56), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
		Utils.DrawBorderStringBig(spriteBatch, $"{mp.Points}", GetRectangle().TopLeft() + new Vector2(32, 56), mp.Points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
		Utils.DrawBorderStringBig(spriteBatch, "Points remaining", GetRectangle().TopLeft() + new Vector2(170, 56), Color.White, 0.6f, 0.5f, 0.35f);
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

	private UIElement Panel => Parent;

	private TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();
	private PassiveTree UITree => UILoader.GetUIState<PassiveTree>();

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
				Vector2 pos = GetDimensions().Center() + Vector2.Lerp(edge.Start.TreePos, edge.End.TreePos, k) + _lineOff;
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
					Vector2 pos = GetDimensions().Center() + Vector2.SmoothStep(edge.Start.TreePos, edge.End.TreePos, progress) + _lineOff;
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
			_blockMouse = Parent.GetDimensions().ToRectangle().Contains(Main.mouseX, Main.mouseY);
			_isHovering = Panel.IsMouseHovering;
		}
		else if (!Main.mouseLeft)
		{
			_blockMouse = _isHovering= false;
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

			Rectangle rec = Parent.GetDimensions().ToRectangle();
			Vector2 adjust = Vector2.Zero;
			Vector2 newOffset = _root + Main.MouseScreen - _start;

			float xAbove = newOffset.X + rec.Width / 2 + UITree.TopLeftTree.X;
			float yAbove = newOffset.Y + rec.Height / 2 + UITree.TopLeftTree.Y;

			float xBelow = newOffset.X - rec.Width / 2 + UITree.BotRightTree.X;
			float yBelow = newOffset.Y - rec.Height / 2 + UITree.BotRightTree.Y;

			if (rec.Height < MathF.Abs(UITree.BotRightTree.Y) + MathF.Abs(UITree.TopLeftTree.Y))
			{
				yAbove = MathF.Max(yAbove, 0);
				yBelow = MathF.Min(yBelow, 0);
			}
			else
			{
				yAbove = MathF.Min(yAbove, 0);
				yBelow = MathF.Max(yBelow, 0);
			}

			if (rec.Width < MathF.Abs(UITree.BotRightTree.X) + MathF.Abs(UITree.TopLeftTree.X))
			{
				xAbove = MathF.Max(xAbove, 0);
				xBelow = MathF.Min(xBelow, 0);
			}
			else
			{
				xAbove = MathF.Min(xAbove, 0);
				xBelow = MathF.Max(xBelow, 0);
			}

			adjust += new Vector2(xAbove, yAbove);
			adjust += new Vector2(xBelow, yBelow);

			_start += adjust;

			foreach (UIElement element in Elements)
			{
				if (element is PassiveElement ele)
				{
					element.Left.Set(ele.Root.X + Main.MouseScreen.X - _start.X, 0.5f);
					element.Top.Set(ele.Root.Y + Main.MouseScreen.Y - _start.Y, 0.5f);
				}
			}

			_lineOff = _root + Main.MouseScreen - _start;
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