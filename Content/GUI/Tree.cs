using FunnyExperience.Core.Loaders.UILoading;
using FunnyExperience.Core.Systems.TreeSystem;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace FunnyExperience.Content.GUI
{
	internal class Tree : SmartUIState
	{
		public bool populated = false;

		public UIPanel panel;

		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!populated)
			{
				panel = new();
				panel.Left.Set(-300, 0.5f);
				panel.Top.Set(-400, 0.5f);
				panel.Width.Set(600, 0);
				panel.Height.Set(800, 0);
				Append(panel);

				var inner = new InnerPanel();
				inner.Left.Set(0, 0);
				inner.Top.Set(0, 0);
				inner.Width.Set(600, 0);
				inner.Height.Set(800, 0);
				panel.Append(inner);

				TreeSystem.nodes.ForEach(n => inner.Append(new PassiveElement(n)));
				populated = true;
			}

			if (Main.LocalPlayer.controlHook)
			{
				RemoveAllChildren();
				populated = false;
			}

			Recalculate();

			base.Draw(spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>("FunnyExperience/Assets/PassiveFrameSmall").Value;
			TreePlayer mp = Main.LocalPlayer.GetModPlayer<TreePlayer>();

			spriteBatch.Draw(tex, panel.GetDimensions().ToRectangle().TopRight() + new Vector2(-32, 32), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderStringBig(spriteBatch, $"{mp.Points}", panel.GetDimensions().ToRectangle().TopRight() + new Vector2(-32, 32), mp.Points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
			Utils.DrawBorderStringBig(spriteBatch, $"Points remaining: ", panel.GetDimensions().ToRectangle().TopRight() + new Vector2(-170, 28), Color.White, 0.6f, 0.5f, 0.35f);
		}
	}

	internal class InnerPanel : SmartUIElement
	{
		public Vector2 start;
		public Vector2 root;
		public Vector2 lineOff;

		UIElement Panel => Parent;

		public override void Draw(SpriteBatch spriteBatch)
		{
			Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
			spriteBatch.GraphicsDevice.ScissorRectangle = Panel.GetDimensions().ToRectangle();

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			foreach (PassiveEdge edge in TreeSystem.edges)
			{
				Texture2D chainTex = ModContent.Request<Texture2D>("FunnyExperience/Assets/Link").Value;

				Color color = Color.Gray;

				if (edge.end.CanAllocate() && edge.start.level > 0)
					color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);

				if (edge.end.level > 0 && edge.start.level > 0)
					color = Color.White;

				for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(edge.start.treePos, edge.end.treePos) / 16))
				{
					Vector2 pos = GetDimensions().Position() + Vector2.Lerp(edge.start.treePos, edge.end.treePos, k) + lineOff;
					Main.spriteBatch.Draw(chainTex, pos, null, color, edge.start.treePos.DirectionTo(edge.end.treePos).ToRotation(), chainTex.Size() / 2, 1, 0, 0);
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
			Main.NewText(GetDimensions().ToRectangle());

			Main.NewText(Left.Pixels);

			passive.Draw(spriteBatch, GetDimensions().Center());

			if (IsMouseHovering)
			{
				Tooltip.SetName($"{passive.name} ({passive.level}/{passive.maxLevel})");
				Tooltip.SetTooltip(passive.tooltip);
			}

			Recalculate();
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (passive.CanAllocate())
			{
				passive.level++;
				Main.LocalPlayer.GetModPlayer<TreePlayer>().Points--;
			}
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			if (passive.CanDeallocate())
			{
				passive.level--;
				Main.LocalPlayer.GetModPlayer<TreePlayer>().Points++;
			}
		}
	}
}
