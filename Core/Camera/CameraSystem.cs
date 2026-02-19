using System.Collections.Generic;

namespace PathOfTerraria.Core.Camera;

/// <summary> Static utility properties and core code for other camera systems. </summary>
[Autoload(Side = ModSide.Client)]
internal sealed class CameraSystem : ModSystem
{
	public delegate void CameraModifierDelegate(Action innerAction);

	private readonly static SortedList<int, CameraModifierDelegate> cameraModifiers = new();

	private static Vector2 lastPositionRemainder;
	private static Vector2 screenCenter;

	public static Vector2 ScreenSize { get; private set; }
	public static Vector2 ScreenHalf { get; private set; }
	public static Rectangle ScreenRect { get; private set; }
	public static Vector2 MouseWorld { get; private set; }
	public static Vector2 ScreenCenter
	{
		get => screenCenter;
		set
		{
			Main.screenPosition = new Vector2(value.X - Main.screenWidth * 0.5f, value.Y - Main.screenHeight * 0.5f);
			UpdateCache();
		}
	}

	public static bool MustSkipCameraUpdate => false; //LimitCameraUpdateRate && TimeSystem.RenderOnlyFrame && !Main.gamePaused;

	public override void Load()
	{
		// Floor camera position, restoring previous remainders before the next camera update.
		// Maximum priority.
		RegisterCameraModifier(int.MaxValue, innerAction =>
		{
			Main.screenPosition += lastPositionRemainder;

			innerAction();

			//var flooredPosition = new Vector2(MathF.Floor(Main.screenPosition.X * 0.5f), MathF.Floor(Main.screenPosition.Y * 0.5f)) * 2f;
			var flooredPosition = new Vector2(MathF.Floor(Main.screenPosition.X), MathF.Floor(Main.screenPosition.Y));

			flooredPosition += Vector2.One;

			lastPositionRemainder = Main.screenPosition - flooredPosition;

			Main.screenPosition = flooredPosition;
		});

		Main.QueueMainThreadAction(() =>
		{
			On_Main.DoDraw_UpdateCameraPosition += orig =>
			{
				if (Main.gameMenu)
				{
					orig();
					PostCameraUpdate();
					return;
				}

				int i = 0;

				void ModifierRecursion()
				{
					int iCopy = i++;

					if (iCopy < cameraModifiers.Count)
					{
						cameraModifiers.Values[iCopy](ModifierRecursion);
					}
					else if (!MustSkipCameraUpdate)
					{
						orig();
					}
				}

				lock (cameraModifiers)
				{
					ModifierRecursion();
				}

				PostCameraUpdate();
			};
		});
	}

	public override void Unload()
	{
		lock (cameraModifiers)
		{
			cameraModifiers.Clear();
		}
	}

	public static void RegisterCameraModifier(int priority, CameraModifierDelegate function)
	{
		lock (cameraModifiers)
		{
			cameraModifiers.Add(-priority, function);
		}
	}

	private static void PostCameraUpdate()
	{
		UpdateCache();
	}

	private static void UpdateCache()
	{
		MouseWorld = Main.MouseWorld;
		ScreenSize = new(Main.screenWidth, Main.screenHeight);
		ScreenHalf = new(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
		ScreenRect = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
		screenCenter = new(Main.screenPosition.X + Main.screenWidth * 0.5f, Main.screenPosition.Y + Main.screenHeight * 0.5f);
	}
}
