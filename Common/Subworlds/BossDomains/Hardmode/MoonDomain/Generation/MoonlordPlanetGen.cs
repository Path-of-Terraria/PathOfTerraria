using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;

internal static class MoonlordPlanetGen
{
	public readonly record struct PlanetTileSet(int Main, int Alt, int Sub, int SubAlt);
	public readonly record struct PlanetInstance(Vector2 Position, int Radius);

	public static PlanetTileSet Solar = new(TileID.LunarBlockSolar, TileID.SolarBrick, TileID.Hellstone, TileID.Obsidian);
	public static PlanetTileSet Nebula = new(TileID.LunarBlockNebula, TileID.NebulaBrick, TileID.VioletMossBlock, TileID.CrystalBlock);
	public static PlanetTileSet Vortex = new(TileID.LunarBlockVortex, TileID.VortexBrick, TileID.MarbleBlock, TileID.BlueDungeonBrick);
	public static PlanetTileSet Stardust = new(TileID.LunarBlockStardust, TileID.StardustBrick, TileID.Sunplate, TileID.GoldStarryGlassBlock);

	internal static void GeneratePlanet(int type, List<PlanetInstance> points)
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		int x;
		int y;

		do
		{
			x = Main.rand.Next(190, Main.maxTilesX - 190);
			y = Main.rand.Next(MoonLordDomain.PlanetTop, MoonLordDomain.CloudTop - 200);
		} while (points.Any(v => new Vector2(x, y).DistanceSQ(v.Position) < MathF.Pow(v.Radius + 80, 2)));

		FastNoiseLite valueNoise = new(WorldGen._genRandSeed);
		valueNoise.SetNoiseType(FastNoiseLite.NoiseType.Value);
		valueNoise.SetFrequency(0.08f);

		FastNoiseLite cellularNoise = new(WorldGen._genRandSeed);
		cellularNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		cellularNoise.SetFrequency(0.055f);

		PlanetTileSet tileSet = type switch
		{
			0 => Vortex,
			1 => Stardust,
			2 => Solar,
			_ => Nebula,
		};

		int size = type switch
		{
			0 => 40,
			1 => 60,
			2 => 80,
			_ => 55,
		};

		size = (int)(size * WorldGen.genRand.NextFloat(0.7f, 1.2f));
		int sizeWithBuffer = size + 20;

		points.Add(new PlanetInstance(new Vector2(x, y), size));

		bool hasSpikes = WorldGen.genRand.NextBool(6);
		int noiseAmp = WorldGen.genRand.Next(3, 7);

		for (int i = x - sizeWithBuffer; i <= x + sizeWithBuffer; i++)
		{
			for (int j = y - sizeWithBuffer; j <= y + sizeWithBuffer; j++)
			{
				if (Vector2.Distance(new Vector2(x, y), new Vector2(i, j)) < size - noise.GetNoise(i, j) * noiseAmp)
				{
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;

					float cellValue = cellularNoise.GetNoise(i, j);

					if (cellValue > -0.7f)
					{
						tile.TileType = (ushort)tileSet.SubAlt;
					}
					else
					{
						tile.TileType = (ushort)(valueNoise.GetNoise(i, j) switch
						{
							< -0.3f => tileSet.Main,
							< 0.1f => tileSet.Alt,
							< 0.5f => tileSet.Sub,
							_ => tileSet.SubAlt
						});
					}

					if (hasSpikes && WorldGen.genRand.NextBool(320))
					{
						var pos = new Vector2(i, j);
						var center = new Vector2(x, y);
						float length = size - pos.Distance(center) + WorldGen.genRand.Next(10, 30);
						float angleRange = WorldGen.genRand.NextFloat(0.15f, 0.35f);
						MoonlordTerrainGen.GenerateSpike(new Point16(i, j), (int)length, pos.AngleFrom(center), angleRange, tileSet.Main, false);
					}
				}
			}
		}
	}
}
