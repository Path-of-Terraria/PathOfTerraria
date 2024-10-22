using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Passes;
internal class SettleLiquidsStep
{
	/// <summary>
	/// Copied from vanilla's Settle Liquids generation step.
	/// </summary>
#pragma warning disable IDE0060 // Remove unused parameter
	public static void Generation(GenerationProgress progress, GameConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		progress.Message = Lang.gen[27].Value;

		Liquid.worldGenTilesIgnoreWater(ignoreSolids: true);
		Liquid.QuickWater(3);
		WorldGen.WaterCheck();
		Liquid.quickSettle = true;

		int repeats = 0;
		int maxRepeats = 10;

		while (repeats < maxRepeats)
		{
			int liquidAmount = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
			repeats++;
			double currentSteps = 0.0;
			int forcedStop = liquidAmount * 5;
			while (Liquid.numLiquid > 0)
			{
				forcedStop--;
				if (forcedStop < 0)
				{
					break;
				}

				double stepsRemaining = (double)(liquidAmount - (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer)) / liquidAmount;

				if (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer > liquidAmount)
				{
					liquidAmount = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
				}

				if (stepsRemaining > currentSteps)
				{
					currentSteps = stepsRemaining;
				}
				else
				{
					stepsRemaining = currentSteps;
				}

				if (repeats == 1)
				{
					progress.Set(stepsRemaining / 3.0 + 0.33);
				}

				Liquid.UpdateLiquid();
			}

			WorldGen.WaterCheck();
			progress.Set(repeats * 0.1 / 3.0 + 0.66);
		}

		Liquid.quickSettle = false;
		Liquid.worldGenTilesIgnoreWater(ignoreSolids: false);
		Main.tileSolid[484] = false;
	}
}
