using PathOfTerraria.Common.World.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;

internal static class MoonlordPlanetGen
{
	public readonly record struct PlanetTileSet(int Main, int Alt, int Sub, int SubAlt);

	public static PlanetTileSet Solar = new(TileID.LunarBlockSolar, TileID.SolarBrick, TileID.Hellstone, TileID.Obsidian);
	public static PlanetTileSet Nebula = new(TileID.LunarBlockNebula, TileID.NebulaBrick, TileID.VioletMossBlock, TileID.CrystalBlock);
	public static PlanetTileSet Vortex = new(TileID.LunarBlockVortex, TileID.VortexBrick, TileID.MarbleBlock, TileID.BlueDungeonBrick);
	public static PlanetTileSet Stardust = new(TileID.LunarBlockStardust, TileID.StardustBrick, TileID.Sunplate, TileID.GoldStarryGlassBlock);

	internal static void GeneratePlanet(int slot, int type)
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
		int y = (int)MathHelper.Lerp(MoonLordDomain.PlanetTop, MoonLordDomain.CloudTop - 200, slot / 3f);

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

		int sizeWithBuffer = size + 20;

		for (int i = x - sizeWithBuffer; i <= x + sizeWithBuffer; i++)
		{
			for (int j = y - sizeWithBuffer; j <= y + sizeWithBuffer; j++)
			{
				if (Vector2.Distance(new Vector2(x, y), new Vector2(i, j)) < size - noise.GetNoise(i, j) * 4)
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
				}
			}
		}
	}
}
