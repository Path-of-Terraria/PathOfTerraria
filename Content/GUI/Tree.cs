using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class Tree : SmartUIState
{
	public bool Populated;

	private UIPanel _panel;
	private UIImageButton _closeButton;

	public bool IsVisible;

	private static TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

	private const int TopPadding = -400;
	private const int LeftPadding = -450;
	private const int PanelWidth = 800;
	private const int PanelHeight = 750;

	public override bool Visible => IsVisible;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public Rectangle GetRectangle()
	{
		return _panel.GetDimensions().ToRectangle();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (Main.LocalPlayer.controlInv)
		{
			IsVisible = false;
		}

		if (!Populated)
		{
			_panel = new UIPanel();
			_panel.Left.Set(LeftPadding, 0.5f);
			_panel.Top.Set(TopPadding, 0.5f);
			_panel.Width.Set(PanelWidth, 0.15f);
			_panel.Height.Set(PanelHeight, 0.15f);
			Append(_panel);

			_closeButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/CloseButton"));
			_closeButton.Left.Set(LeftPadding + PanelWidth - 120, 0.5f);
			_closeButton.Top.Set(TopPadding + 10, 0.5f);
			_closeButton.Width.Set(38, 0);
			_closeButton.Height.Set(38, 0);
			_closeButton.OnLeftClick += (a, b) => {
				IsVisible = false;
				SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
			};
			_closeButton.SetVisibility(1, 1);
			Append(_closeButton);

			var inner = new InnerPanel();
			inner.Left.Set(0, 0);
			inner.Top.Set(0, 0);
			inner.Width.Set(PanelWidth - 0, 0);
			inner.Height.Set(PanelHeight - 0, 0);
			_panel.Append(inner);

			TreeSystem.Nodes.ForEach(n => inner.Append(new PassiveElement(n)));
			Populated = true;
		}

		Recalculate();

		base.Draw(spriteBatch);

		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/PassiveFrameSmall").Value;
		TreePlayer mp = Main.LocalPlayer.GetModPlayer<TreePlayer>();

		spriteBatch.Draw(tex, GetRectangle().TopLeft() + new Vector2(32, 32), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
		Utils.DrawBorderStringBig(spriteBatch, $"{mp.Points}", GetRectangle().TopLeft() + new Vector2(32, 32), mp.Points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
		Utils.DrawBorderStringBig(spriteBatch, $"Points remaining", GetRectangle().TopLeft() + new Vector2(170, 32), Color.White, 0.6f, 0.5f, 0.35f);
	}
}

internal class InnerPanel : SmartUIElement
{
	private Vector2 _start;
	private Vector2 _root;
	private Vector2 _lineOff;

	private UIElement Panel => Parent;

	private TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

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

			if (edge.End.Level <= 0 || edge.Start.Level <= 0)
			{
				continue;
			}

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

	private bool _blockMouse;
	private bool _isHovering;
	private bool _lastState;
	
	public override void SafeUpdate(GameTime gameTime)
	{
		switch (Main.mouseLeft)
		{
			case true when !_lastState:
				_blockMouse = GetDimensions().ToRectangle().Contains(Main.mouseX, Main.mouseY);
				_isHovering = Panel.IsMouseHovering;
				break;
			case false:
				_blockMouse = _isHovering= false;
				break;
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

			foreach (UIElement element in Elements)
			{
				if (element is not PassiveElement ele)
				{
					continue;
				}

				element.Left.Set(ele.Root.X + Main.MouseScreen.X - _start.X, 0);
				element.Top.Set(ele.Root.Y + Main.MouseScreen.Y - _start.Y, 0);
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

internal class PassiveElement : SmartUIElement
{
	private readonly Passive _passive;
	public Vector2 Root;

	private int _flashTimer;
	private int _redFlashTimer;

	public PassiveElement(Passive passive)
	{
		_passive = passive;
		Left.Set(passive.TreePos.X - passive.Width / 2, 0);
		Top.Set(passive.TreePos.Y - passive.Height / 2, 0);
		Width.Set(passive.Width, 0);
		Height.Set(passive.Height, 0);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_passive.Draw(spriteBatch, GetDimensions().Center());

		if (_flashTimer > 0)
		{
			Texture2D glow = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/StarAlpha").Value;

			float prog = _flashTimer / 20f;

			var glowColor = new Color(255, 230, 150)
			{
				A = 0
			};

			glowColor *= prog * 0.5f;

			spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
			spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + (1f - prog), 0, 0);

			_flashTimer--;
		}

		if (_redFlashTimer > 0)
		{
			Texture2D glow = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/StarAlpha").Value;

			float prog = _redFlashTimer / 20f;

			var glowColor = new Color(255, 60, 60)
			{
				A = 0
			};

			glowColor *= prog * 0.5f;

			spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
			spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + prog, 0, 0);

			_redFlashTimer--;
		}

		if (IsMouseHovering)
		{
			string name = _passive.Name;

			if (_passive.MaxLevel > 1)
			{
				name += $" ({_passive.Level}/{_passive.MaxLevel})";
			}

			Tooltip.SetName(name);
			Tooltip.SetTooltip(_passive.Tooltip);
		}

		Recalculate();
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!_passive.CanAllocate(Main.LocalPlayer))
		{
			return;
		}

		_passive.Level++;
		Main.LocalPlayer.GetModPlayer<TreePlayer>().Points--;

		_flashTimer = 20;

		switch (_passive.MaxLevel)
		{
			case 1:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 2:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 3:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 5:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 6:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					case 6: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 7:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					case 6: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 7: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			default:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				break;
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (!_passive.CanDeallocate(Main.LocalPlayer))
		{
			return;
		}

		_passive.Level--;
		Main.LocalPlayer.GetModPlayer<TreePlayer>().Points++;

		_redFlashTimer = 20;

		SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
	}
}