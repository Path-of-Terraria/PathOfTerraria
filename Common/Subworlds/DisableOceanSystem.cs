using SubworldLibrary;
using Terraria.Audio;

namespace PathOfTerraria.Common.Subworlds;

internal class DisableOceanSystem : ModSystem
{
	public override void Load()
	{
		On_LegacyAudioSystem.UpdateMisc += StopMusic;
	}

	private void StopMusic(On_LegacyAudioSystem.orig_UpdateMisc orig, LegacyAudioSystem self)
	{
		orig(self);

		if (SubworldSystem.Current is IOverrideOcean ocean && Main.LocalPlayer.ZoneBeach)
		{
			ocean.OverrideOcean();
		}
	}
}
