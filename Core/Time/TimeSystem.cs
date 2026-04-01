using PathOfTerraria.Utilities;
using System.Reflection;

namespace PathOfTerraria.Core.Time;

internal sealed class TimeSystem : ModSystem
{
	private static uint lastRenderUpdateCount;

	/// <summary> The game's fixed tick rate. </summary>
	public static int LogicFramerate => 60;
	/// <summary> The last logical update's total elapsed GameTime in seconds. </summary>
	public static float LogicTime { get; private set; }
	/// <summary> One divided by the game's fixed tick rate. </summary>
	public static float LogicDeltaTime => 1f / LogicFramerate;
	/// <summary> Logical update count, including those during which the game has been paused. </summary>
	public static ulong UpdateCount { get; private set; }
	
	/// <summary> The last render's total elapsed GameTime in seconds. </summary>
	public static float RenderTime { get; private set; }
	/// <summary> The last render's delta elapsed GameTime in seconds. </summary>
	public static float RenderDeltaTime { get; private set; } = 1f / 60f;
	/// <summary> Whether the current render has not been accompanied by a logical update. </summary>
	public static bool RenderOnlyFrame { get; private set; }

	public override void Load()
	{
		Main.OnTickForInternalCodeOnly += OnTickForInternalCodeOnly;
		On_Main.DoUpdate_WhilePaused += OnDoUpdateWhilePaused;

		// Hooking of potentially executing draw methods has to be done on the main thread.
		Main.QueueMainThreadAction(static () =>
		{
			// Cache LogicTime before updating
			ILUtils.EmitILDetour(typeof(Main).GetMethod("DoUpdate", BindingFlags.Instance | BindingFlags.NonPublic), (Main main, ref GameTime gameTime) =>
			{
				LogicTime = (float)gameTime.TotalGameTime.TotalSeconds;
			}, null);

			// Cache some drawing information before drawing
			ILUtils.EmitILDetour(typeof(Main).GetMethod("DoDraw", BindingFlags.Instance | BindingFlags.NonPublic), (Main main, GameTime gameTime) => // Set render info
			{
				uint updateCount = Main.GameUpdateCount;

				RenderOnlyFrame = updateCount == lastRenderUpdateCount;
				lastRenderUpdateCount = updateCount;

				RenderTime = (float)gameTime.TotalGameTime.TotalSeconds;
				RenderDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			}, 
			(Main main, GameTime gameTime) => // and then unset flag
			{
				RenderOnlyFrame = false;
			});
		});
	}

	public override void Unload()
	{
		Main.OnTickForInternalCodeOnly -= OnTickForInternalCodeOnly;
	}

	private static void OnTickForInternalCodeOnly()
	{
		UpdateCount = unchecked(UpdateCount + 1);
	}
	private static void OnDoUpdateWhilePaused(On_Main.orig_DoUpdate_WhilePaused orig)
	{
		UpdateCount = unchecked(UpdateCount + 1);
		orig();
	}
}
