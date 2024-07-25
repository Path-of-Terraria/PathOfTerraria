using System.Collections.Generic;

namespace PathOfTerraria.Common.World.Generation.Town;

internal abstract class TownGenerationBase
{
	protected readonly List<Rectangle> PlacedBuildings = [];

	public abstract void Place(Rectangle bounds, bool center, params Building[] buildings);
}