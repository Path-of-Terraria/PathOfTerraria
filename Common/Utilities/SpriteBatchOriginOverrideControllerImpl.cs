using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;

namespace PathOfTerraria.Common.Utilities;

internal readonly struct SpriteBatchOriginOverrideController : IDisposable
{
	private sealed class SpriteBatchOriginOverrideControllerImpl : ILoadable
	{
		private delegate void PushSpriteDelegate(
			SpriteBatch sb,
			Texture2D texture,
			float sourceX,
			float sourceY,
			float sourceW,
			float sourceH,
			float destinationX,
			float destinationY,
			float destinationW,
			float destinationH,
			Color color,
			float originX,
			float originY,
			float rotationSin,
			float rotationCos,
			float depth,
			byte effects);

		void ILoadable.Load(Mod mod)
		{
			MonoModHooks.Add(
				typeof(SpriteBatch).GetMethod("PushSprite", BindingFlags.Instance | BindingFlags.NonPublic),
				PushSpriteWithModifiedOrigin);
		}

		void ILoadable.Unload()
		{
			// We could also check to make sure origins.Count is 0.
			origins = null;
		}

		private static void PushSpriteWithModifiedOrigin(
			PushSpriteDelegate orig,
			SpriteBatch sb,
			Texture2D texture,
			float sourceX,
			float sourceY,
			float sourceW,
			float sourceH,
			float destinationX,
			float destinationY,
			float destinationW,
			float destinationH,
			Color color,
			float originX,
			float originY,
			float rotationSin,
			float rotationCos,
			float depth,
			byte effects)
		{
			if (origins.Count != 0)
			{
				Main.NewText(origins.Peek().X);
				Vector2 origin = origins.Peek();
				originX = origin.X / sourceW / texture.Width;
				originY = origin.Y / sourceH / texture.Height;
			}

			orig(sb, texture, sourceX, sourceY, sourceW, sourceH, destinationX, destinationY, destinationW, destinationH, color, originX, originY, rotationSin, rotationCos, depth, effects);
		}
	}

	private static Stack<Vector2> origins = [];

	public SpriteBatchOriginOverrideController(Vector2 origin)
	{
		origins.Push(origin);
	}

	void IDisposable.Dispose()
	{
		if (origins.Count == 0)
		{
			throw new InvalidOperationException("Could not dispose SpriteBatchOriginOverrideController because there was no origin to pop from the stack!");
		}

		origins.Pop();
	}
}
