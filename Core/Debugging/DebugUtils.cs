using System.Collections.Generic;

namespace PathOfTerraria.Core.Debugging;

public sealed class DebugUtils : ModSystem
{
	private static readonly List<Action<SpriteBatch>> debugDraws = [];

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
		debugDraws.Add(action);
	}

	private static void PostDraw(GameTime obj)
	{
		if (debugDraws.Count != 0)
		{
			try
			{
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

				foreach (Action<SpriteBatch> action in debugDraws)
				{
					action(Main.spriteBatch);
				}
			}
			finally
			{
				debugDraws.Clear();
				Main.spriteBatch.End();
			}
		}
	}
}