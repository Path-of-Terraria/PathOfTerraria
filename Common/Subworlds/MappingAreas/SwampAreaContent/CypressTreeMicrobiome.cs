using PathOfTerraria.Common.World.Generation;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

internal class CypressTreeMicrobiome : MicroBiome
{
	const float BottomLeafHeightProportion = 0.7f;
	const int MinimumTrunkWidth = 6;

	public static float SizeModifier = 1f;

	public override bool Place(Point origin, StructureMap structures)
	{
		if (!structures.CanPlace(new Rectangle(origin.X - 30, origin.Y - 30, 60, 60)))
		{
			return false;
		}

		origin.Y += 10;

		BuildTrunk(origin, out int topY, out int height);

		if (height != 0)
		{
			BuildLeaves(origin, topY, height, out Range leafRange);
			structures.AddProtectedStructure(new Rectangle(leafRange.Start.Value, topY, leafRange.End.Value - leafRange.Start.Value, origin.X - topY), -30);
		}

		return true;
	}

	private static void BuildLeaves(Point origin, int topY, int height, out Range leafRange)
	{
		float heightModifier = (height / 200f);
		int bottomLeavesY = (int)MathHelper.Lerp(origin.Y, topY, BottomLeafHeightProportion);
		int leafCount = _random.Next(2, 4);
		int width = (int)(_random.Next(10, 17) * heightModifier);
		leafRange = 0..0;

		for (int i = 0; i < leafCount; ++i)
		{
			LeafPlacement(origin, topY, heightModifier, bottomLeavesY, leafCount, ref width, i, out leafRange, 1f);
		}

		leafCount = 6;
		width = (int)(_random.Next(3, 8) * heightModifier);

		for (int i = 0; i < leafCount; ++i)
		{
			LeafPlacement(origin, topY, heightModifier, bottomLeavesY, leafCount, ref width, i, out _, 0.45f);
		}

		for (int i = 0; i < 30; ++i)
		{
			int x = origin.X + _random.Next(-MinimumTrunkWidth, MinimumTrunkWidth);
			WorldGen.TileRunner(x, (int)MathHelper.Lerp(topY, bottomLeavesY, _random.NextFloat()), _random.NextFloat(7, 12), 8, TileID.LeafBlock, true, overRide: _random.NextBool());
		}
	}

	private static void LeafPlacement(Point origin, int topY, float heightModifier, int bottomLeavesY, int leafCount, ref int width, int i, out Range leafRange, float globalSizeModifier = 1f)
	{
		int y = (int)MathHelper.Lerp(topY, bottomLeavesY, i / (leafCount - 1f));
		float size = _random.NextFloat(0.65f, 0.8f);

		if (i == leafCount - 1)
		{
			size = 1f;
		}
		else if (i == 0)
		{
			size = 0.6f;
		}

		int useWidth = i == 0 ? (int)(width * _random.NextFloat(1.5f, 2f)) : width;
		int minX = Main.maxTilesX;
		int maxX = 0;

		for (int x = origin.X - (int)(useWidth * globalSizeModifier); x < origin.X + useWidth * globalSizeModifier; ++x)
		{
			float sizeMod = (1 - Math.Abs(x - origin.X) / (float)useWidth * 0.75f) + 0.25f;
			WorldGen.TileRunner(x, y, sizeMod * 12 * size * globalSizeModifier, 8, TileID.LeafBlock, true);

			if (_random.NextBool(3))
			{
				int range = (int)MathF.Max(sizeMod * 8 * size, 1);
				WorldGen.TileRunner(x, y - _random.Next(-range, range), sizeMod * 14 * size * globalSizeModifier, 8, TileID.LeafBlock, true, overRide: i == leafCount - 1);
			}

			minX = Math.Min(x, minX);
			maxX = Math.Max(x, maxX);
		}

		leafRange = minX..maxX;
		width += (int)(_random.Next(10, 20) * heightModifier);

		if (i == leafCount - 2)
		{
			width = (int)(width * _random.NextFloat(1.15f, 1.5f));
		}
	}

	private static void BuildTrunk(Point origin, out int topY, out int height)
	{
		int widthLeft = _random.Next(20, 45);
		int widthRight = _random.Next(20, 45);
		bool hadLiquid = false;
		height = _random.Next(200, 280);
		topY = origin.Y - height;

		while (Main.tile[origin.X, (int)MathHelper.Lerp(origin.Y, topY, BottomLeafHeightProportion)].LiquidAmount > 0)
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

		for (int y = origin.Y; y > topY; --y)
		{
			Tile tile = Main.tile[origin.X, y];
			
			if (!tile.HasTile && tile.LiquidAmount > 0)
			{
				hadLiquid = true;
			}
		}

		if (!hadLiquid)
		{
			height = 0;
			return;
		}

		height = (int)(height * SizeModifier);
		topY = origin.Y - height;
		widthLeft = (int)(widthLeft * SizeModifier);
		widthRight = (int)(widthRight * SizeModifier);

		int rootCount = (int)(15 * SizeModifier);

		for (int i = 0; i < rootCount; ++i)
		{
			PlaceRoot(new Point(origin.X + (int)(MathHelper.Lerp(-widthLeft * 1.3f, widthRight, i / (rootCount - 1f)) * 0.8f), origin.Y));
		}

		FastNoiseLite noise = new(_random.Next());

		for (int y = origin.Y; y > topY; --y)
		{
			int distance = y - topY;
			float factor = MathF.Pow(distance / (float)height, 4);
			int leftWidth = (int)(MathHelper.Lerp(MinimumTrunkWidth, widthLeft, factor));
			int rightWidth = (int)(MathHelper.Lerp(MinimumTrunkWidth, widthRight, factor));

			for (int x = origin.X - leftWidth; x < origin.X + rightWidth; ++x)
			{
				Tile tile = Main.tile[x, y];
				tile.HasTile = true;
				tile.TileType = TileID.LivingWood;

				bool actuate = y > SwampArea.HeightMapping[x] - Math.Abs(noise.GetNoise(x, y) * 6) - 10 && y <= SwampArea.HeightMapping[x] - 5;

				if (!actuate && tile.LiquidAmount > 0)
				{
					actuate = y > SwampArea.HeightMapping[x] - Math.Abs(noise.GetNoise(x, y) * 15) - 40 && y <= SwampArea.HeightMapping[x] - 30;
				}

				tile.IsActuated = actuate;
			}
		}

		static void PlaceRoot(Point origin)
		{
			WorldUtils.Gen(origin, new ShapeRoot(MathHelper.PiOver2 + _random.NextFloat(-0.2f, 0.2f), _random.NextFloat(20, 80), 8, 1),
				Actions.Chain(new Actions.ClearTile(false), new Actions.PlaceTile(TileID.LivingWood)));
		}
	}
}
