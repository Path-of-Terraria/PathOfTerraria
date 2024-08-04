using System.Linq;

namespace PathOfTerraria.Common.World.Generation.Town;

internal class CanopyTown : TownGenerationBase
{
	public override void Place(Rectangle bounds, bool center, params Building[] buildings)
	{
		if (center)
		{
			bounds.X -= bounds.Width / 2;
			bounds.Y -= bounds.Height / 2;
		}

		PlacedBuildings.Clear();

		int attempts = 0;

		for (int i = 0; i < buildings.Length; i++)
		{
			attempts++;

			if (attempts > 500)
			{
				attempts = 0;
				continue;
			}

			Building building = buildings[i];
			Point location;

			do
			{
				location = new Point(bounds.X + WorldGen.genRand.Next(bounds.Width), bounds.Y + WorldGen.genRand.Next(bounds.Height));
				attempts++;

				if (attempts > 500)
				{
					break;
				}
			} while (PlacedBuildings.Any(x => x.Intersects(new Rectangle(location.X, location.Y, building.Size.X, building.Size.Y))));

			if (attempts > 500)
			{
				attempts = 0;
				continue;
			}

			if (building.PlaceAt(location))
			{
				PlacedBuildings.Add(building.GetBoundingBox(location));
				attempts = 0;
			}
			else
			{
				i--;
				attempts++;
			}
		}
	}
}