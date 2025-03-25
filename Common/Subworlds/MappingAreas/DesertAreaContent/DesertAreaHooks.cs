namespace PathOfTerraria.Common.Subworlds.MappingAreas.DesertAreaContent;

internal class DesertAreaHooks : ModSystem
{
	public override void Load()
	{
		On_WorldGen.GrowCactus += StopIfOOB;
	}

	private void StopIfOOB(On_WorldGen.orig_GrowCactus orig, int i, int j)
	{
		if (!WorldGen.InWorld(i, j, Main.offLimitBorderTiles))
		{
			return;
		}

		try
		{
			orig(i, j);
		}
		catch
		{
		}
	}
}
