#define DEBUG_LIGHTING

using System.Diagnostics.CodeAnalysis;
using MonoMod.Cil;
using ReLogic.Threading;
using PathOfTerraria.Core.Camera;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.Utilities;
using Terraria.GameContent;

//TODO:
// TML may introduce a shared implementation of this in the future,
// watch out for it and switch to it when available, in order to reduce processing and memory overhead.

#nullable enable
#pragma warning disable IDE0042 // Deconstruct variable declaration

namespace PathOfTerraria.Core.Lighting;

internal sealed class Surface<T>(int width, int height) : IDisposable where T : unmanaged
{
	public T[] Data { get; private set; } = new T[width * height];
	public int Width { get; private set; } = width;
	public int Height { get; private set; } = height;

	public ref T this[int x, int y]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref Data[Index(x, y)];
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Data = null!;
		Width = -1;
		Height = -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Index(int x, int y)
	{
		return Width * y + x;
	}
}

/// <summary>
/// Creates and maintains a small screen-bound buffer containing lighting information.
/// </summary>
[Autoload(Side = ModSide.Client)]
internal sealed class LightingBuffer : ModSystem
{
	private static bool buffersCreated;
	private static bool buffersFilled;
	private static uint lastLightingUpdateCount;
	private static RenderTarget2D? screenSpaceTexture;
	private static RenderTarget2D? tileSpaceTexture;
	private static Surface<Color>? tileSpaceColors;
	private static Vector2 lastCaptureScreenPosition;

	private static int UpdateFrequency => 5;
	private static int ExtraTilesOffset => 4;
	private static int CaptureThreadCount => Math.Min(4, Environment.ProcessorCount / 2);

	public override void Load()
	{
		// This fixes tileTarget not being available in many cases. And other dumb issues.
		MonoModHooks.Add
		(
			typeof(Main).GetProperty(nameof(Main.RenderTargetsRequired))!.GetMethod!,
			new Func<Func<bool>, bool>(orig => true)
		);

		Main.QueueMainThreadAction(() =>
		{
			Main.OnPreDraw += OnPreDraw;
			IL_Main.DoDraw += DoDrawInjection;
		});
	}
	private static void DoDrawInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);
		il.GotoNext
		(
			MoveType.After,
			i => i.MatchCall(typeof(Main), "DoDraw_UpdateCameraPosition")
		);
		il.EmitDelegate(PostUpdateCameraPosition);
	}

	public override void Unload()
	{
		Main.OnPreDraw -= OnPreDraw;
		DeinitBuffers();
	}
	public override void PostDrawInterface(SpriteBatch sb)
	{
		DebugLighting(sb);
	}

	private static void OnPreDraw(GameTime obj)
	{
		if (NeedsBufferInit())
		{
			InitBuffers();
		}
	}
	private static void PostUpdateCameraPosition()
	{
		if (NeedsBufferInit())
		{
			InitBuffers();
		}

		if (ShouldCaptureLighting())
		{
			CaptureLighting();
		}

		TransferLighting();
	}

	private static Vector2Int GetBufferSize()
	{
		int offset2 = ExtraTilesOffset + ExtraTilesOffset;
		return new(
			(int)Math.Ceiling(Main.screenWidth / (float)TileUtils.TileSizeInPixels) + offset2,
			(int)Math.Ceiling(Main.screenHeight / (float)TileUtils.TileSizeInPixels) + offset2
		);
	}

	private static bool NeedsBufferInit()
	{
		return !buffersCreated || Main.screenWidth != screenSpaceTexture!.Width || Main.screenHeight != screenSpaceTexture.Height || GetBufferSize() != (Vector2Int)tileSpaceTexture.Size();
	}

	private static void InitBuffers()
	{
		ThreadUtils.RunOnMainThread(static () =>
		{
			Vector2Int tileSpace = GetBufferSize();
			var screenSpace = new Vector2Int(Main.screenWidth, Main.screenHeight);

			tileSpaceColors?.Dispose();
			tileSpaceTexture?.Dispose();
			screenSpaceTexture?.Dispose();

			tileSpaceColors = new Surface<Color>(tileSpace.X, tileSpace.Y);
			tileSpaceTexture = new RenderTarget2D(Main.graphics.GraphicsDevice, tileSpace.X, tileSpace.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			screenSpaceTexture = new RenderTarget2D(Main.graphics.GraphicsDevice, screenSpace.X, screenSpace.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			// Initialize with data to prevent driver-specific issues.
			TextureUtils.InitializeWithColor(tileSpaceTexture, Color.DarkViolet);
			TextureUtils.InitializeWithColor(screenSpaceTexture, Color.OrangeRed);
			buffersCreated = true;
		});
	}
	private static void DeinitBuffers()
	{
		if (!buffersCreated) { return; }

		buffersCreated = false;

		if (tileSpaceTexture != null)
		{
			lock (tileSpaceTexture)
			{
				RenderTarget2D textureHandle = tileSpaceTexture;
				ThreadUtils.RunOnMainThread(textureHandle.Dispose);
				tileSpaceTexture = null;
			}
		}

		tileSpaceColors?.Dispose();
		tileSpaceColors = null;
	}

	private static bool ShouldCaptureLighting()
	{
		return !buffersFilled || (Main.GameUpdateCount != lastLightingUpdateCount && (Main.GameUpdateCount % UpdateFrequency) == 0);
	}

	// Populates tilespace buffer.
	private static void CaptureLighting()
	{
		if (!buffersCreated) { return; }

		int offset1 = ExtraTilesOffset;
		int offset2 = offset1 + offset1;

		(float X, float Y, float Width, float Height) bufferAreaF =
		(
			(Main.screenPosition.X / TileUtils.TileSizeInPixels) - offset1,
			(Main.screenPosition.Y / TileUtils.TileSizeInPixels) - offset1,
			MathF.Ceiling(Main.screenWidth / (float)TileUtils.TileSizeInPixels) + offset2,
			MathF.Ceiling(Main.screenHeight / (float)TileUtils.TileSizeInPixels) + offset2
		);

		var bufferArea = new Rectangle(
			(int)bufferAreaF.X,
			(int)bufferAreaF.Y,
			(int)bufferAreaF.Width,
			(int)bufferAreaF.Height
		);
		var bufferPosRemainder = new Vector2(
			bufferAreaF.X - bufferArea.X,
			bufferAreaF.Y - bufferArea.Y
		);

		Color[] bufferArray = tileSpaceColors!.Data;

		FastParallel.For(0, CaptureThreadCount, (int threadId, int numThreads, object context) =>
		{
			Span<Color> bufferData = bufferArray;
			int stepsPerThread = bufferData.Length / numThreads;
			int start = threadId * stepsPerThread;
			int end = start + stepsPerThread;
			(int y, int x) = Math.DivRem(start, bufferArea.Width);

			for (int i = start; i < end; i++)
			{
				bufferData[i] = Terraria.Lighting.GetColor(bufferArea.X + x, bufferArea.Y + y);

				x += 1;
				if (x == bufferArea.Width)
				{
					x = 0;
					y += 1;
				}
			}
		});

		lock (tileSpaceTexture!)
		{
			tileSpaceTexture.SetData(bufferArray);
		}

		buffersFilled = true;
		lastLightingUpdateCount = Main.GameUpdateCount;
		lastCaptureScreenPosition = CameraSystem.ScreenCenter - (bufferPosRemainder * TileUtils.TileSizeInPixels);
	}

	/// <summary> Returns the lighting buffer if it is available, otherwise a white texture. </summary>
	public static Texture2D GetOrWhite()
	{
		return TryGet(out Texture2D? result) ? result : TextureAssets.MagicPixel.Value;
	}
	/// <summary> Attemps to acquire the lighting buffer if it is available. </summary>
	public static bool TryGet([NotNullWhen(true)] out Texture2D? result)
	{
		if (buffersCreated && buffersFilled && screenSpaceTexture is { } lighting)
		{
			result = lighting;
			return true;
		}

		result = default;
		return false;
	}

	// Renders tilespace capture onto the screenspace buffer.
	private static void TransferLighting()
	{
		SpriteBatch sb = Main.spriteBatch;
		GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;

		graphicsDevice.SetRenderTarget(screenSpaceTexture);
		//graphicsDevice.Clear(Color.DarkViolet);

		var screenSpaceTargetSize = new Vector2Int(
			screenSpaceTexture!.Width,
			screenSpaceTexture!.Height
		);
		var tileSpaceTargetSizeInPixels = new Vector2Int(
			tileSpaceTexture!.Width * TileUtils.TileSizeInPixels,
			tileSpaceTexture!.Height * TileUtils.TileSizeInPixels
		);
		Vector2 moveOffset = lastCaptureScreenPosition - CameraSystem.ScreenCenter;
		Vector2Int diffOffset = -(tileSpaceTargetSizeInPixels - screenSpaceTargetSize) / 2;
		Vector2 totalOffset = moveOffset + diffOffset;

		Rectangle dstRect;
		dstRect.X = (int)totalOffset.X;
		dstRect.Y = (int)totalOffset.Y;
		dstRect.Width = tileSpaceTargetSizeInPixels.X;
		dstRect.Height = tileSpaceTargetSizeInPixels.Y;

		sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
		sb.Draw(tileSpaceTexture, dstRect, Color.White);
		sb.End();

		graphicsDevice.SetRenderTarget(null);
	}

	private static void DebugLighting(SpriteBatch sb)
	{
#if DEBUG_LIGHTING
		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad5))
		{
			var dstA = new Rectangle(32, Main.screenHeight / 2 - 64, Main.screenWidth / 2 - 64, Main.screenHeight / 2 - 64);
			var dstB = new Rectangle(Main.screenWidth / 2 + 32, Main.screenHeight / 2 - 64, Main.screenWidth / 2 - 64, Main.screenHeight / 2 - 64);

			sb.Draw(screenSpaceTexture, dstA, Color.White);
			sb.Draw(tileSpaceTexture, dstB, Color.White);
			DebugUtils.DrawRectInUI(dstA, Color.Yellow);
			DebugUtils.DrawRectInUI(dstB, Color.Beige);
		}
		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad6))
		{
			var dst = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			sb.Draw(screenSpaceTexture, dst, null, Color.White with { A = 192 });
		}
#endif
	}
}
