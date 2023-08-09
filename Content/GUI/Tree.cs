using FunnyExperience.Core.Loaders.UILoading;
using FunnyExperience.Core.Systems.TreeSystem;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace FunnyExperience.Content.GUI
{
	internal class Tree : SmartUIState
	{
		public bool populated = false;

		public UIPanel panel;
		public UIImageButton closeButton;

		public bool visible;

		public TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

		public override bool Visible => visible;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (Main.LocalPlayer.controlInv)
				visible = false;

			if (!populated)
			{
				panel = new();
				panel.Left.Set(-300, 0.5f);
				panel.Top.Set(-400, 0.5f);
				panel.Width.Set(600, 0);
				panel.Height.Set(800, 0);
				Append(panel);

				closeButton = new(ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/CloseButton"));
				closeButton.Left.Set(252, 0.5f);
				closeButton.Top.Set(-392, 0.5f);
				closeButton.Width.Set(38, 0);
				closeButton.Height.Set(38, 0);
				closeButton.OnLeftClick += (a, b) => visible = false;
				closeButton.SetVisibility(1, 1);
				Append(closeButton);

				var inner = new InnerPanel();
				inner.Left.Set(0, 0);
				inner.Top.Set(0, 0);
				inner.Width.Set(600, 0);
				inner.Height.Set(800, 0);
				panel.Append(inner);

				TreeSystem.nodes.ForEach(n => inner.Append(new PassiveElement(n)));
				populated = true;
			}

			Recalculate();

			base.Draw(spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/PassiveFrameSmall").Value;
			TreePlayer mp = Main.LocalPlayer.GetModPlayer<TreePlayer>();

			spriteBatch.Draw(tex, panel.GetDimensions().ToRectangle().TopLeft() + new Vector2(32, 32), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderStringBig(spriteBatch, $"{mp.Points}", panel.GetDimensions().ToRectangle().TopLeft() + new Vector2(32, 32), mp.Points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
			Utils.DrawBorderStringBig(spriteBatch, $"Points remaining", panel.GetDimensions().ToRectangle().TopLeft() + new Vector2(170, 32), Color.White, 0.6f, 0.5f, 0.35f);
		}
	}

	internal class InnerPanel : SmartUIElement
	{
		public Vector2 start;
		public Vector2 root;
		public Vector2 lineOff;

		UIElement Panel => Parent;

		public TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

		public override void Draw(SpriteBatch spriteBatch)
		{
			Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
			spriteBatch.GraphicsDevice.ScissorRectangle = Panel.GetDimensions().ToRectangle();

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			foreach (PassiveEdge edge in TreeSystem.edges)
			{
				Texture2D chainTex = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/Link").Value;

				Color color = Color.Gray;

				if (edge.end.CanAllocate(Main.LocalPlayer) && edge.start.level > 0)
					color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);

				if (edge.end.level > 0 && edge.start.level > 0)
					color = Color.White;

				for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(edge.start.treePos, edge.end.treePos) / 16))
				{
					Vector2 pos = GetDimensions().Position() + Vector2.Lerp(edge.start.treePos, edge.end.treePos, k) + lineOff;
					Main.spriteBatch.Draw(chainTex, pos, null, color, edge.start.treePos.DirectionTo(edge.end.treePos).ToRotation(), chainTex.Size() / 2, 1, 0, 0);
				}

				if (edge.end.level > 0 && edge.start.level > 0)
				{
					Texture2D glow = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/GlowAlpha").Value;
					var glowColor = new Color(255, 230, 150)
					{
						A = 0
					};

					var rand = new Random(edge.GetHashCode());

					for (int k = 0; k < 8; k++)
					{
						float dist = Vector2.Distance(edge.start.treePos, edge.end.treePos);
						float len = (40 + rand.Next(120)) * dist / 50;
						float scale = 0.05f + rand.NextSingle() * 0.15f;

						float progress = (Main.GameUpdateCount + 15 * k) % len / (float)len;
						Vector2 pos = GetDimensions().Position() + Vector2.SmoothStep(edge.start.treePos, edge.end.treePos, progress) + lineOff;
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

		public override void SafeUpdate(GameTime gameTime)
		{
			if (Main.mouseLeft && Panel.IsMouseHovering)
			{
				if (start == Vector2.Zero)
				{
					start = Main.MouseScreen;
					root = lineOff;

					foreach (UIElement element in Elements)
					{
						if (element is PassiveElement ele)
							ele.root = new Vector2(ele.Left.Pixels, ele.Top.Pixels);
					}
				}

				foreach (UIElement element in Elements)
				{
					if (element is PassiveElement ele)
					{
						element.Left.Set(ele.root.X + Main.MouseScreen.X - start.X, 0);
						element.Top.Set(ele.root.Y + Main.MouseScreen.Y - start.Y, 0);
					}
				}

				lineOff = root + Main.MouseScreen - start;
			}
			else
			{
				start = Vector2.Zero;
			}

			Recalculate();
		}
	}

	internal class PassiveElement : SmartUIElement
	{
		public Passive passive;
		public Vector2 root;

		public int flashTimer;
		public int redFlashTimer;

		public PassiveElement(Passive passive)
		{
			this.passive = passive;
			Left.Set(passive.treePos.X - passive.width / 2, 0);
			Top.Set(passive.treePos.Y - passive.height / 2, 0);
			Width.Set(passive.width, 0);
			Height.Set(passive.height, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			passive.Draw(spriteBatch, GetDimensions().Center());

			if (flashTimer > 0)
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/GlowAlpha").Value;
				Texture2D star = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/StarAlpha").Value;

				float prog = flashTimer / 20f;

				var glowColor = new Color(255, 230, 150)
				{
					A = 0
				};

				glowColor *= prog * 0.5f;

				spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
				spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + (1f - prog), 0, 0);

				flashTimer--;
			}

			if (redFlashTimer > 0)
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/GlowAlpha").Value;
				Texture2D star = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/StarAlpha").Value;

				float prog = redFlashTimer / 20f;

				var glowColor = new Color(255, 60, 60)
				{
					A = 0
				};

				glowColor *= prog * 0.5f;

				spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
				spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + prog, 0, 0);

				redFlashTimer--;
			}

			if (IsMouseHovering)
			{
				string name = passive.name;

				if (passive.maxLevel > 1)
					name += $" ({passive.level}/{passive.maxLevel})";

				Tooltip.SetName(name);
				Tooltip.SetTooltip(passive.tooltip);
			}

			Recalculate();
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (passive.CanAllocate(Main.LocalPlayer))
			{
				passive.level++;
				Main.LocalPlayer.GetModPlayer<TreePlayer>().Points--;

				flashTimer = 20;

				if (passive.maxLevel == 1)
				{
					switch (passive.level)
					{
						case 1: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
						default: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
					}

					return;
				}

				if (passive.maxLevel == 2)
				{
					switch (passive.level)
					{
						case 1: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier2")); break;
						case 2: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
						default: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
					}

					return;
				}

				if (passive.maxLevel == 3)
				{
					switch (passive.level)
					{
						case 1: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier1")); break;
						case 2: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier3")); break;
						case 3: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
						default: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
					}

					return;
				}

				if (passive.maxLevel == 5)
				{
					switch (passive.level)
					{
						case 1: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier1")); break;
						case 2: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier2")); break;
						case 3: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier3")); break;
						case 4: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier4")); break;
						case 5: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
						default: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
					}

					return;
				}

				if (passive.maxLevel == 6)
				{
					switch (passive.level)
					{
						case 1: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier1")); break;
						case 2: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier2")); break;
						case 3: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier3")); break;
						case 4: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier4")); break;
						case 5: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
						case 6: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier1")); break;
						default: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
					}

					return;
				}

				if (passive.maxLevel == 7)
				{
					switch (passive.level)
					{
						case 1: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier1")); break;
						case 2: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier2")); break;
						case 3: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier3")); break;
						case 4: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier4")); break;
						case 5: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
						case 6: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier2")); break;
						case 7: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier1")); break;
						default: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
					}

					return;
				}

				switch (passive.level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{FunnyExperience.ModName}/Sounds/Tier5")); break;
				}
			}
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			if (passive.CanDeallocate(Main.LocalPlayer))
			{
				passive.level--;
				Main.LocalPlayer.GetModPlayer<TreePlayer>().Points++;

				redFlashTimer = 20;

				SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
			}
		}
	}
}
