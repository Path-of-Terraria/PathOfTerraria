using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Core.WorldGeneration.TownGeneration;

internal abstract class TownGenerationBase
{
	protected readonly List<Rectangle> PlacedBuildings = [];

	public abstract void Place(Rectangle bounds, bool center, params Building[] buildings);
}
