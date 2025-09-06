namespace PathOfTerraria.Core.Time;

internal sealed class TimeSystem : ModSystem
{
	/// <summary> Logical update count, including those during which the game has been paused. </summary>
	public static ulong UpdateCount { get; private set; }

	public override void Load()
	{
		Main.OnTickForInternalCodeOnly += OnTickForInternalCodeOnly;
		On_Main.DoUpdate_WhilePaused += OnDoUpdateWhilePaused;
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
