using SubworldLibrary;
using Terraria.Audio;

namespace PathOfTerraria.Common.Subworlds;

internal class DisableOceanSystem : ModSystem
{
	public override void Load()
	{
		On_LegacyAudioSystem.UpdateMisc += OveriddeOceanPostMusic;
		On_Player.UpdateBiomes += OverrideOceanPostUpdateBiomes;
	}

	private void OverrideOceanPostUpdateBiomes(On_Player.orig_UpdateBiomes orig, Player self)
	{
		orig(self);

		if (SubworldSystem.Current is IOverrideOcean ocean && Main.LocalPlayer.ZoneBeach)
		{
			ocean.OverrideOcean();
		}
	}

	private void OveriddeOceanPostMusic(On_LegacyAudioSystem.orig_UpdateMisc orig, LegacyAudioSystem self)
	{
		orig(self);

		if (SubworldSystem.Current is IOverrideOcean ocean && Main.LocalPlayer.ZoneBeach)
		{
			ocean.OverrideOcean();
		}
	}
}
