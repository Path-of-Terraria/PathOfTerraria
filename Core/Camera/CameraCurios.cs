using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.Graphics;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Xna;
using PathOfTerraria.Common.Utilities;

#nullable enable

namespace PathOfTerraria.Core.Camera;

/// <summary> Description used to instance a camera curio - a locational camera effect. </summary>
internal struct CameraCurio()
{
	/// <summary> The string ID that should be used to identify this curio. </summary>
	public required string Identifier { get; set; }
	/// <summary> How strong should the camera be pulled towards this curio. </summary>
	public required float Weight { get; set; }
	/// <summary> How much time in seconds should this curio exist for. </summary>
	public required float LengthInSeconds { get; set; }
	/// <summary> The curio's position in world. </summary>
	public required Vector2 Position { get; set; }
	/// <summary> The curio's active range, including a pow factor. </summary>
	public ExponentialRange? Range { get; set; } = null;
	/// <summary> How fast should the curio's effect begin when it is created, in seconds. </summary>
	public float FadeInLength { get; set; } = 0.5f;
	/// <summary> How fast should the curio's effect stop when it stop existing, in seconds. </summary>
	public float FadeOutLength { get; set; } = 0.5f;
	/// <summary> The target zoom value that this curio should apply, if any. </summary>
	public float? Zoom { get; set; } = null;
	/// <summary> A callback that may be used to update the curio's position. </summary>
	public Func<Vector2?>? Callback { get; set; }
}

/// <summary> Implementation of locational camera effects. </summary>
[Autoload(Side = ModSide.Client)]
internal sealed class CameraCurios : ModSystem
{
	public struct CameraCurioInstance()
	{
		public float StartTime;
		public float EndTime;
		public float Intensity;
		public Vector2 Position;
		public CameraCurio Style;

		public readonly bool Active => TimeSystem.RenderTime >= StartTime && TimeSystem.RenderTime <= EndTime;
	}

	private static readonly List<CameraCurioInstance> curios = [];
	private static WeightedValue<float, float> lastCalculatedZoom;

	public override void Load()
	{
		CameraSystem.RegisterCameraModifier(-200, ApplyCameraModifier);
		Main.QueueMainThreadAction(static () => Main.OnPostDraw += PostDraw);
	}
	public override void Unload()
	{
		Main.QueueMainThreadAction(static () => Main.OnPostDraw -= PostDraw);
	}

	private static void PostDraw(GameTime gameTime)
	{
		Update(TimeSystem.RenderDeltaTime);
	}

	private static void Update(float deltaTime)
	{
		if (Main.gamePaused || !Main.hasFocus) { return; }

		foreach (ref CameraCurioInstance curio in CollectionsMarshal.AsSpan(curios))
		{
			float intensityTarget = curio.Active ? 1f : 0f;
			float fadePeriod = curio.Active ? curio.Style.FadeInLength : curio.Style.FadeOutLength;
			fadePeriod = 1f;
			curio.Intensity = fadePeriod <= 0f ? intensityTarget : MathUtils.StepTowards(curio.Intensity, intensityTarget, (1f / fadePeriod) * deltaTime);

			if (curio.Style.Callback?.Invoke() is { } newPosition) { curio.Position = newPosition; }
		}

		curios.RemoveAll(i => !i.Active & i.Intensity <= 0f);
	}

	public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
	{
		// Apply weighted zoom.
		if (lastCalculatedZoom.TotalWeight > 0f)
		{
			float offset = (float)lastCalculatedZoom.Total();
			transform.Zoom = new Vector2(
				MathHelper.Clamp(transform.Zoom.X + offset, 1f, 2f),
				MathHelper.Clamp(transform.Zoom.Y + offset, 1f, 2f)
			);
		}
	}

	public static void Create(in CameraCurio style)
	{
		if (Main.dedServ)
		{
			return;
		}

		CameraCurioInstance instance;
		instance.Style = style;
		instance.Position = style.Position;
		instance.StartTime = TimeSystem.RenderTime;
		instance.EndTime = instance.StartTime + style.LengthInSeconds;
		instance.Intensity = 0f;

		if (style.Identifier is string identifier && curios.FindIndex(i => i.Style.Identifier == identifier) is (>= 0 and int index))
		{
			curios[index] = instance with
			{
				Intensity = curios[index].Intensity,
			};
			return;
		}

		curios.Add(instance);
	}

	private static void ApplyCameraModifier(Action innerAction)
	{
		innerAction();

		if (CameraSystem.MustSkipCameraUpdate) { return; }

		Vector2 baseCameraPoint = Main.LocalPlayer.Center; //CameraSystem.ScreenCenter;
		var offset = new WeightedValue2D<float, float>((0, 0), (0, 0), (1, 1));
		var zoom = new WeightedValue<float, float>(0, 0, 1);

		foreach (ref readonly CameraCurioInstance curio in CollectionsMarshal.AsSpan(curios))
		{
			Vector2 targetOffset = curio.Position - baseCameraPoint;
			float intensity = curio.Intensity;

			if (curio.Style.Range.HasValue)
			{
				intensity *= curio.Style.Range.Value.DistanceFactor(curio.Position.Distance(baseCameraPoint));
			}

			float posWeight = curio.Style.Weight * intensity;
			float zoomWeight = intensity;

			if (posWeight > 0)
			{
				offset.Add((targetOffset.X, targetOffset.Y), (posWeight, posWeight));
			}

			if (curio.Style.Zoom is { } zoomValue && zoomWeight > 0f)
			{
				zoom.Add(zoomValue, zoomWeight);
			}
		}

		if (offset.X.TotalWeight > 0 || offset.Y.TotalWeight > 0)
		{
			(float x, float y) = offset.Total();
			Main.screenPosition += new Vector2(x, y).ToPoint().ToVector2();
		}

		lastCalculatedZoom = zoom;
	}
}
