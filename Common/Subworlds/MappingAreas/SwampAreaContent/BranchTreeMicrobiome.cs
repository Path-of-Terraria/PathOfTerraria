using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.Maps.Swamp;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

#nullable enable

internal class BranchTreeMicrobiome : MicroBiome
{
	public readonly record struct LeafInstance(Point16 Position, float SizeModifier, float Angle);

	public override bool Place(Point origin, StructureMap structures)
	{
		int width = _random.Next(160, 300);
		FastNoiseLite noise = new(_random.Next());
		List<Vector2> canopy = GenerateCanopy(origin, width, noise);
		Dictionary<Vector2, float> branchTips = [];
		GenerateBranches(canopy, noise, branchTips);
		GenerateRoots(canopy, noise, width, origin);
		GenerateLeaves(branchTips, canopy);
		GeneratePlatforms(canopy);
		GetEncounterPosition(canopy);

		return true;
	}

	private static void GetEncounterPosition(List<Vector2> canopy)
	{
		Vector2 maxY = canopy.MaxBy(x => x.Y);
		float midX = (canopy.MaxBy(x => x.X).X + canopy.MinBy(x => x.X).X) / 2f;

		SwampArea.EncounterLocations.Add(new Vector2(midX, maxY.Y));
	}

	private static void GeneratePlatforms(List<Vector2> canopy)
	{
		int count = _random.Next(11, 19);
		List<Vector2> taken = [];
		int skip = 0;
		List<Point16> chestPlacementOptions = [];

		for (int i = 0; i < count; ++i)
		{
			Vector2 randomPointOnCanopy;
			int stuckCount = 0;

			do
			{
				randomPointOnCanopy = _random.Next(canopy);

				int yOffset = (int)MathHelper.Lerp(30, 200, MathF.Pow(_random.NextFloat(), 8));

				randomPointOnCanopy.Y += yOffset;
				stuckCount++;

				if (stuckCount > 1500)
				{
					return;
				}
			} while (taken.Any(x => randomPointOnCanopy.DistanceSQ(x) < 20 * 20));

			int width = _random.Next(14, 21);
			HashSet<Point16> platformPositions = [];

			for (int x = (int)(randomPointOnCanopy.X - width / 2); x < randomPointOnCanopy.X + width / 2; ++x)
			{
				int y = (int)randomPointOnCanopy.Y;
				Tile tile = Main.tile[x, y];

				if (!tile.HasTile && tile.LiquidAmount <= 0 && WorldUtils.Find(new Point(x, y), new Searches.Up(y - 5).Conditions(new Conditions.IsSolid()), out Point foundPos) && foundPos.Y > 5)
				{
					platformPositions.Add(new Point16(x, y));
				}
				else
				{
					skip++;
					goto skipAll;
				}
			}

			taken.Add(randomPointOnCanopy);

			short minPlatformX = platformPositions.Min(x => x.X);
			short maxPlatformX = platformPositions.Max(x => x.X);

			foreach (Point16 pos in platformPositions)
			{
				WorldGen.PlaceTile(pos.X, pos.Y, TileID.Platforms, true);

				if (pos.X == minPlatformX || pos.X == maxPlatformX)
				{
					Tile tile = Main.tile[pos.X, pos.Y - 1];
					int y = -1;

					while (!tile.HasTile)
					{
						WorldGen.PlaceTile(pos.X, pos.Y + y, TileID.VineRope, true);
						y--;
						tile = Main.tile[pos.X, pos.Y + y];
					}

					if (tile.TileType == TileID.Platforms)
					{
						WorldGen.PlaceTile(pos.X, pos.Y + y - 1, TileID.VineRope, true);
					}
				}
			}

			foreach (Point16 pos in platformPositions)
			{
				if (pos.X != minPlatformX && pos.X != maxPlatformX)
				{
					chestPlacementOptions.Add(new Point16(pos.X, pos.Y - 1));
				}
			}

			continue;

			skipAll:

			if (skip > 15000)
			{
				break;
			}

			i--;
			continue;
		}

		for (int i = 0; i < 2; ++i)
		{
			Point16 pos = _random.Next(chestPlacementOptions);
			int chest = WorldGen.PlaceChest(pos.X, pos.Y, TileID.Containers, false, 12);

			if (chest == -1)
			{
				i--;
				continue;
			}
			else
			{
				PopulateChest(chest);
			}
		}
	}

	private static void PopulateChest(int chestIndex)
	{
		Chest chest = Main.chest[chestIndex];

		WeightedRandom<(int type, Range stackRange)> miscChestLoot = new();
		miscChestLoot.Add((AutomaticItemContent.AutoItemType<SwampMoss>(), 9..15), 1f);
		miscChestLoot.Add((AutomaticItemContent.AutoItemType<PurpleClouds>(), 9..15), 1f);
		miscChestLoot.Add((AutomaticItemContent.AutoItemType<DeepMoss>(), 9..15), 1f);

		Tile tile = Main.tile[chest.x, chest.y];
		List<ItemDatabase.ItemRecord> drops = DropTable.RollManyMobDrops(3, PoTItemHelper.PickItemLevel(), 1f, random: WorldGen.genRand);

		if (tile.HasTile && TileID.Sets.BasicChest[tile.TileType])
		{
			for (int k = 0; k < 5; ++k)
			{
				if (k < 3)
				{
					ItemDatabase.ItemRecord drop = drops[k];
					chest.item[k] = new Item(drop.ItemId, drop.Item.stack);
				}
				else
				{
					(int type, Range stackRange) = miscChestLoot.Get();
					chest.item[k] = new Item(type, Main.rand.Next(stackRange.Start.Value, stackRange.End.Value + 1));
				}
			}
		}
	}

	private static void GenerateLeaves(Dictionary<Vector2, float> branchTips, List<Vector2> canopy)
	{
		int leafCount = 6;

		foreach ((Vector2 branchTip, float branchAngle) in branchTips)
		{
			List<LeafInstance> moreLeaves = [];

			for (int i = 0; i < leafCount; ++i)
			{
				float angle = branchAngle + _random.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);
				GenPlacement.GenerateLeaf(branchTip, _random.NextFloat(10, 16), _random.NextFloat(12, 20), angle, (x, y, a) => PlaceLeaf(x, y, a, moreLeaves, 1), false);
			}

			while (moreLeaves.Count > 0)
			{
				List<LeafInstance> leaves = [];

				foreach (LeafInstance instance in moreLeaves)
				{
					float angle = instance.Angle + _random.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);
					float width = _random.NextFloat(10, 16) * instance.SizeModifier;
					float height = _random.NextFloat(12, 20) * instance.SizeModifier;
					GenPlacement.GenerateLeaf(instance.Position.ToVector2(), width, height, angle, (x, y, a) => PlaceLeaf(x, y, a, leaves, instance.SizeModifier), false);
				}

				moreLeaves.Clear();

				foreach (LeafInstance instance in leaves)
				{
					moreLeaves.Add(instance);
				}
			}
		}

		for (int i = 0; i < 40; ++i)
		{
			Vector2 randomPointOnCanopy = _random.Next(canopy);
			float width = _random.NextFloat(3, 12);

			GenPlacement.GenerateLeaf(randomPointOnCanopy, width, width * _random.NextFloat(1.5f, 3f), _random.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2), 
				(x, y, a) => PlaceLeaf(x, y, a, null, 0), false);
		}
	}

	private static void PlaceLeaf(int x, int y, float angle, List<LeafInstance>? moreLeaves, float size)
	{
		Tile tile = Main.tile[x, y];
		tile.HasTile = true;
		tile.TileType = TileID.LeafBlock;

		if (_random.NextBool(60) && moreLeaves is not null)
		{
			moreLeaves.Add(new(new Point16(x, y), size * _random.NextFloat(0.9f, 1f), angle));
		}
	}

	private static void GenerateRoots(List<Vector2> canopy, FastNoiseLite noise, int width, Point origin)
	{
		const float MinSize = 3;
		const float MaxSize = 7;

		int branchCount = _random.Next(3, 6);
		List<Vector2> taken = [];
		List<(Vector2, float)> rootPlacements = [];

		for (int i = 0; i < branchCount; ++i)
		{
			Vector2 randomPointOnCanopy;
			int stuckCount = 0;

			do
			{
				randomPointOnCanopy = _random.Next(canopy);
				stuckCount++;

				if (stuckCount > 1500)
				{
					goto placement;
				}
			} while (taken.Any(x => Math.Abs(randomPointOnCanopy.X - x.X) < 15));

			const int RootHorizontalDisplacementRange = 80;

			Vector2 midPoint = randomPointOnCanopy + new Vector2(_random.Next(-10, 10), _random.Next(20, 60));
			float widthFactor = MathHelper.Clamp(Utils.GetLerpValue(origin.X - width / 2, origin.X + width / 2, randomPointOnCanopy.X, true) + _random.NextFloat(-0.1f, 0.1f), 0, 1);
			Vector2 searchPoint = midPoint + new Vector2(MathHelper.Lerp(-RootHorizontalDisplacementRange, RootHorizontalDisplacementRange + 20, widthFactor), 10);

			if (!WorldUtils.Find(searchPoint.ToPoint(), new Searches.Down(500).Conditions(new Conditions.IsSolid()), out Point bottom))
			{
				continue;
			}

			taken.Add(randomPointOnCanopy);

			Vector2 bottomPos = bottom.ToVector2() + new Vector2(0, 10);
			List<Vector2> currentPoints = [.. Tunnel.GenerateBezier([randomPointOnCanopy, midPoint, bottomPos], 4, 0)];

			float minY = currentPoints.Min(x => x.Y);
			float maxY = currentPoints.Max(x => x.Y);

			foreach (Vector2 point in currentPoints)
			{
				float size = MathHelper.Lerp(MinSize, MaxSize, Utils.GetLerpValue(minY, maxY, point.Y, true));
				rootPlacements.Add((point, size));
			}
		}

		placement:

		foreach ((Vector2 point, float size) in rootPlacements)
		{
			GenPlacement.TileCircle(point, size + noise.GetNoise(point.X, point.Y) * 0.8f, (x, y) =>
			{
				Tile tile = Main.tile[x, y];
				tile.TileType = TileID.LivingWood;
				tile.HasTile = true;
				tile.IsActuated = true;
			}, GenPlacement.Replaceability.Cuttable);
		}
	}

	private static void GenerateBranches(List<Vector2> canopy, FastNoiseLite noise, Dictionary<Vector2, float> branchTips)
	{
		int branchCount = _random.Next(5, 11);
		List<Vector2> taken = [];
		int repeatCount = 0;

		for (int i = 0; i < branchCount; ++i)
		{
			Vector2 randomPointOnCanopy;

			do
			{
				randomPointOnCanopy = _random.Next(canopy);

				if (repeatCount++ > 3000)
				{
					return;
				}
			} while (taken.Any(x => x.DistanceSQ(randomPointOnCanopy) < 12 * 12));

			Vector2 midPoint = randomPointOnCanopy + new Vector2(_random.Next(-40, 40), _random.Next(-25, -10));
			Vector2 tip = midPoint - new Vector2(_random.Next(-20, 20), _random.Next(10, 20));
			List<Vector2> currentPoints = [.. Tunnel.GenerateBezier([randomPointOnCanopy, midPoint, tip], 8, 0)];
			taken.Add(randomPointOnCanopy);

			for (int j = 0; j < currentPoints.Count; j++)
			{
				Vector2 point = currentPoints[j];
				GenPlacement.TileCircle(point, (2 + noise.GetNoise(point.X, point.Y) * 0.8f) * (1.4f - j / (float)currentPoints.Count), TileID.LivingWood);
			}

			branchTips.TryAdd(tip, tip.AngleFrom(currentPoints[^5]));
		}
	}

	private static List<Vector2> GenerateCanopy(Point origin, int width, FastNoiseLite noise)
	{
		List<Vector2> controls = [];
		int controlCount = _random.Next(8, 18);

		for (int i = 0; i < controlCount; ++i)
		{
			Vector2 pos;
			int repeats = 0;

			do
			{
				pos = new Vector2(origin.X + _random.Next(-width / 2, width / 2), origin.Y + _random.Next(-30, 30));
				repeats++;

				if (repeats > 15000)
				{
					goto cut;
				}
			} while (controls.Any(x => MathF.Abs(pos.X - x.X) < 20));

			controls.Add(pos);
		}

		cut:

		controls = [.. controls.OrderBy(x => x.X)];
		List<Vector2> total = [];

		GenerateSegmentCanopyBranch(controls, noise, total);

		float topY = controls.Min(x => x.Y);
		Rectangle structure = new((int)controls[0].X, (int)topY, (int)(controls[^1].X - controls[0].X), (int)(Main.maxTilesY - topY));
		GenVars.structures.AddProtectedStructure(structure, 10);
		return total;
	}

	private static void GenerateSegmentCanopyBranch(List<Vector2> controls, FastNoiseLite noise, List<Vector2> total)
	{
		for (int i = 0; i < controls.Count - 2; ++i)
		{
			Vector2 centerPoint = Vector2.Lerp(controls[i], controls[i + 1], 0.5f) + _random.NextVector2Circular(50, 50);
			List<Vector2> currentPoints = [.. Tunnel.GenerateBezier([controls[i], centerPoint, controls[i + 1]], 8, 0)];

			foreach (Vector2 point in currentPoints)
			{
				GenPlacement.TileCircle(point, 4 + noise.GetNoise(point.X, point.Y) * 2, TileID.LivingWood);
			}

			total.AddRange(currentPoints);
		}
	}
}
