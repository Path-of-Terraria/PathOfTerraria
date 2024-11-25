using PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;
using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Passes;

internal class FrozenRuinPass : AutoGenStep
{
	public override void Generate(GenerationProgress progress, GameConfiguration config)
	{
		int ruinId = WorldGen.genRand.Next(3);
		string path = "Assets/Structures/DeerclopsDomain/FrozenRuin_" + ruinId;
		Point16 size = StructureTools.GetSize(path);

		while (true)
		{
			int x = WorldGen.genRand.Next(Main.maxTilesX / 6, Main.maxTilesX / 6 * 5);
			int y = WorldGen.genRand.Next((int)Main.worldSurface, Main.maxTilesY / 5 * 2);

			Tile tile = Main.tile[x, y];

			if (tile.HasTile && (tile.TileType == TileID.IceBlock || tile.TileType == TileID.SnowBlock) &&
				GenVars.structures.CanPlace(new Rectangle(x, y, size.X, size.Y)))
			{
				StructureTools.PlaceByOrigin(path, new Point16(x, y), Vector2.Zero);
				ModContent.GetInstance<DeerclopsSystem>().AntlerLocation = new Point16(x + size.X / 2, y + size.Y / 2);
				GenVars.structures.AddStructure(new Rectangle(x, y, size.X, size.Y), 3);

				break;
			}
		}
	}

	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.FindIndex(x => x.Name == "Smooth World");
	}
}
