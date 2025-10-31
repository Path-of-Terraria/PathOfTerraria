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
			On_Main.DoUpdate += OnDoUpdate;
			On_Main.DoDraw += OnDoDraw;
		});
	}
	public override void Unload()
	{
		Main.OnTickForInternalCodeOnly -= OnTickForInternalCodeOnly;
	}

	private static void OnDoUpdate(On_Main.orig_DoUpdate orig, Main main, ref GameTime gameTime)
	{
		LogicTime = (float)gameTime.TotalGameTime.TotalSeconds;

		orig(main, ref gameTime);
	}
	private static void OnDoDraw(On_Main.orig_DoDraw orig, Main main, GameTime gameTime)
	{
		uint updateCount = Main.GameUpdateCount;

		RenderOnlyFrame = updateCount == lastRenderUpdateCount;
		lastRenderUpdateCount = updateCount;

		RenderTime = (float)gameTime.TotalGameTime.TotalSeconds;
		RenderDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		orig(main, gameTime);

		RenderOnlyFrame = false;
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
