using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds;

internal class DisableOceanSystem : ModSystem
{
	public override void Load()
	{
		On_Player.UpdateBiomes += StopOceanInSubworlds;
	}

	private void StopOceanInSubworlds(On_Player.orig_UpdateBiomes orig, Player self)
	{
		orig(self);

		if (SubworldSystem.Current is IOverrideOcean ocean)
		{
			self.ZoneBeach = false;
			ocean.OnOceanOverriden();
		}
	}
}
