namespace PathOfTerraria.Core.Items;

internal sealed class ItemHookLoadState : ModSystem
{
	public static bool GlobalItemHooksReady { get; private set; }

	public override void Load()
	{
		GlobalItemHooksReady = false;
	}

	public override void PostSetupContent()
	{
		GlobalItemHooksReady = true;
	}

	public override void Unload()
	{
		GlobalItemHooksReady = false;
	}
}
