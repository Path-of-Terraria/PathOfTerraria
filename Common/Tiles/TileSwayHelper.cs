namespace PathOfTerraria.Common.Tiles;

internal static class TileSwayHelper
{
	//Adapted from vanilla - should be used for multitiles
	internal static float GetHighestWindGridPushComplex(int topLeftX, int topLeftY, int sizeX, int sizeY, int totalPushTime, float pushForcePerFrame, int loops, bool swapLoopDir)
	{
		float result = 0f;
		int num = int.MaxValue;

		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				Main.instance.TilesRenderer.Wind.GetWindTime(topLeftX + i, topLeftY + j, totalPushTime, out int windTimeLeft, out _, out _);
				float windGridPushComplex = Main.instance.TilesRenderer.GetWindGridPushComplex(topLeftX + i, topLeftY + j, totalPushTime, pushForcePerFrame, loops, swapLoopDir);

				if (windTimeLeft < num && windTimeLeft != 0)
				{
					result = windGridPushComplex;
					num = windTimeLeft;
				}
			}
		}

		return result;
	}
}