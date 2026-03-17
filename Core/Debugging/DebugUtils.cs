using System.Collections.Generic;
using Terraria.GameContent;

namespace PathOfTerraria.Core.Debugging;

public sealed class DebugUtils : ModSystem
{
	private static readonly List<Action<SpriteBatch>> uiDraws = [];
	private static readonly List<Action<SpriteBatch>> worldDraws = [];

	public override void Load()
	{
		Main.OnPostDraw += PostDraw;
	}
	public override void Unload()
	{
		Main.OnPostDraw -= PostDraw;
	}

	public static void DrawInWorld(Action<SpriteBatch> action)
	{
		worldDraws.Add(action);
	}
	public static void DrawInUI(Action<SpriteBatch> action)
	{
		worldDraws.Add(action);
	}

	public static void DrawRectangle(SpriteBatch sb, Rectangle rect, Color color, int lineWidth = 1)
	{
		Texture2D tex = TextureAssets.BlackTile.Value;
		(int x1, int y1, int x2, int y2, int rw, int rh, int lw)
			= (rect.X, rect.Y, rect.Right, rect.Bottom, rect.Width, rect.Height, lineWidth);

		sb.Draw(tex, new Rectangle(x1, y1, rw, lw), color);
		sb.Draw(tex, new Rectangle(x1, y2, rw, lw), color);
		sb.Draw(tex, new Rectangle(x1, y1, lw, rh), color);
		sb.Draw(tex, new Rectangle(x2, y1, lw, rh), color);
	}

	private static void PostDraw(GameTime obj)
	{
		DrawList(worldDraws, Main.GameViewMatrix.TransformationMatrix);
		DrawList(uiDraws, Main.UIScaleMatrix);
	}

	private static void DrawList(List<Action<SpriteBatch>> actions, Matrix matrix)
	{
		if (actions.Count == 0) { return; }

		try
		{
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, matrix);

			foreach (Action<SpriteBatch> action in actions)
			{
				action(Main.spriteBatch);
			}
		}
		finally
		{
			Main.spriteBatch.End();
			actions.Clear();
		}
	}
}