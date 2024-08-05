using System.Collections.Generic;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World;

internal abstract class AutoGenStep : ModType
{
	public static List<AutoGenStep> Steps = [];

	public virtual string GenName => GetType().Name;

	protected override void Register()
	{
		Steps.Add(this);
		ModTypeLookup<AutoGenStep>.Register(this);
	}

	public abstract int GenIndex(List<GenPass> tasks);
	public abstract void Generate(GenerationProgress progress, GameConfiguration config);
}
