using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds;

internal class OverrideBiomeWorld : ModSystem
{
	public override void Load()
	{
		On_Main.UpdateAudio += OverrideBiomes;
		On_Player.UpdateBiomes += OverrideBiomes;
	}

	private void OverrideBiomes(On_Main.orig_UpdateAudio orig, Main self)
	{
		orig(self);

		if (SubworldSystem.Current is IOverrideBiomeWorld ocean)
		{
			ocean.OverrideBiome();
		}
	}

	private void OverrideBiomes(On_Player.orig_UpdateBiomes orig, Player self)
	{
		orig(self);

		if (SubworldSystem.Current is IOverrideBiomeWorld ocean)
		{
			ocean.OverrideBiome();
		}
	}
}
