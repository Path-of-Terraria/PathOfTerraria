using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

internal class CypressTreeMicrobiome : MicroBiome
{
	public override bool Place(Point origin, StructureMap structures)
	{
		BuildTrunk(origin, out int topY, out int height);
		BuildLeaves(origin, topY, height);
		return true;
	}

	private static void BuildLeaves(Point origin, int topY, int height)
	{
		float heightModifier = (height / 250f);
		int bottomLeavesY = (int)MathHelper.Lerp(origin.Y, topY, 0.6f);
		int leafCount = _random.Next(5, 8);
		int width = (int)(_random.Next(2, 8) * heightModifier);

		for (int i = 0; i < leafCount; ++i)
		{
			int y = (int)MathHelper.Lerp(topY, bottomLeavesY, i / (leafCount - 1f));
			float size = i == leafCount -1 ? 1 : _random.NextFloat(0.25f, 0.4f);

			for (int x = origin.X - width; x < origin.X + width; x += 2)
			{
				float sizeMod = (1 - Math.Abs(x - origin.X) / (float)width * 0.75f) + 0.25f;
				WorldGen.TileRunner(x, y, sizeMod * 20, 8, TileID.LeafBlock, true);

				if (_random.NextBool(3))
				{
					int range = (int)(sizeMod * 12);
					WorldGen.TileRunner(x, y - _random.Next(-range, range), sizeMod * 16 * size, 8, TileID.LeafBlock, true, overRide: i == leafCount - 1);
				}
			}

			width += (int)(_random.Next(10, 20) * heightModifier);

			if (i == leafCount - 2)
			{
				width = (int)(width * _random.NextFloat(1.15f, 1.5f));
			}
		}
	}

	private static void BuildTrunk(Point origin, out int topY, out int height)
	{
		const int MinWidth = 6;

		int widthLeft = _random.Next(20, 45);
		int widthRight = _random.Next(20, 45);
		bool hadLiquid = false;
		height = _random.Next(60, 220);
		topY = origin.Y - height;

		while (Main.tile[origin.X, (int)MathHelper.Lerp(origin.Y, topY, 0.6f)].LiquidAmount > 0)
		{
			height += 20;
			topY = origin.Y - height;
			hadLiquid = true;
		}

		if (hadLiquid)
		{
			height += _random.Next(160, 180);
			topY = origin.Y - height;
		}

		for (int i = 0; i < 8; ++i)
		{
			PlaceRoot(new Point(origin.X + (int)(_random.Next(-widthLeft, widthRight) * 0.8f), origin.Y));
		}

		PlaceRoot(new Point(origin.X - (int)(widthLeft), origin.Y));
		PlaceRoot(new Point(origin.X + widthRight - 8, origin.Y));

		for (int y = origin.Y; y > topY; --y)
		{
			int distance = y - topY;
			float factor = MathF.Pow(distance / (float)height, 4);
			int leftWidth = (int)(MathHelper.Lerp(MinWidth, widthLeft, factor));
			int rightWidth = (int)(MathHelper.Lerp(MinWidth, widthRight, factor));

			for (int x = origin.X - leftWidth; x < origin.X + rightWidth; ++x)
			{
				Tile tile = Main.tile[x, y];
				tile.HasTile = true;
				tile.TileType = TileID.LivingWood;
			}
		}

		static void PlaceRoot(Point origin)
		{
			WorldUtils.Gen(origin, new ShapeRoot(MathHelper.PiOver2 + _random.NextFloat(-0.2f, 0.2f), _random.NextFloat(20, 80), 8, 1),
				Actions.Chain(new Actions.ClearTile(false), new Actions.PlaceTile(TileID.LivingWood)));
		}
	}
}
