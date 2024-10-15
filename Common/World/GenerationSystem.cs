using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World;

internal class GenerationSystem : ModSystem
{
	public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
	{
		foreach (AutoGenStep item in AutoGenStep.Steps)
		{
			int index = item.GenIndex(tasks);

			if (index >= 0)
			{
				tasks.Insert(index, new PassLegacy(item.Name, item.Generate));
			}
		}
	}
}
