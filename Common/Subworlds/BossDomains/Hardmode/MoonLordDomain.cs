using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class MoonLordDomain : BossDomainSubworld
{
	public override int Width => 900;
	public override int Height => 3000;
	public override (int time, bool isDay) ForceTime => (3500, false);

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override void Load()
	{
		On_Item.NewItem_Inner += BlockDropsInThisDomain;
	}

	private int BlockDropsInThisDomain(On_Item.orig_NewItem_Inner orig, IEntitySource source, int X, int Y, int Width, int Height, Item itemToClone, int Type, int Stack, 
		bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
	{
		int item = orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);

		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return item;
		}

		Main.item[item].active = false;
		return item;
	}

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain)];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = Height / 2;
		Main.worldSurface = Height - 50;
		Main.rockLayer = Height - 40;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		noise.SetFrequency(0.014f);
		noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
		noise.SetFractalOctaves(2);
		noise.SetFractalGain(-2.570f);
		noise.SetFractalWeightedStrength(-0.066f);
		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.BasicGrid);
		noise.SetDomainWarpAmp(175);

		const int RangeStep = 40; 

		Dictionary<int, int> closestValidTileLookup = [];
		int rangeSpace = TileLoader.TileCount / RangeStep;
		int baseRange = Main.rand.Next(rangeSpace + 15, rangeSpace * (RangeStep - 1) - 15);
		Range range = (baseRange - rangeSpace)..(baseRange + rangeSpace);

		for (int i = Math.Max(range.Start.Value - 10, 0); i < Math.Min(range.End.Value + 10, TileLoader.TileCount); ++i)
		{
			int id = i;
			
			if (InvalidId(id))
			{
				SearchForBetterId(ref id);
			}

			closestValidTileLookup.Add(i, id);
		}

		for (int i = 0; i < Width; ++i)
		{
			float x = i;
			float throwawayY = 0;

			noise.DomainWarp(ref x, ref throwawayY);

			for (int j = 2000 - (int)(noise.GetNoise(x, throwawayY) * 40); j < Height; ++j)
			{
				x = i;
				float y = j;

				noise.DomainWarp(ref x, ref y);

				Tile tile = Main.tile[i, j];
				tile.HasTile = true;
				tile.TileType = (ushort)GetNearestTileId(noise, x, y, closestValidTileLookup, range);
			}
		}
	}

	private static int GetNearestTileId(FastNoiseLite noise, float x, float y, Dictionary<int, int> closestValidTileLookup, Range idRange)
	{
		float noiseValue = noise.GetNoise(x, y);
		int id = (int)MathHelper.Lerp(idRange.Start.Value, idRange.End.Value, Utils.GetLerpValue(-1.3f, 0.7f, noiseValue, true));
		return closestValidTileLookup[id];
	}

	private static void SearchForBetterId(ref int id)
	{
		int topId = id;

		while (InvalidId(topId))
		{
			if (topId > TileLoader.TileCount - 1)
			{
				topId = -1;
				break;
			}

			topId++;
		}

		int bottomId = id;

		while (InvalidId(bottomId))
		{
			bottomId--;

			if (bottomId == -1)
			{
				break;
			}
		}

		if (topId == -1 || Math.Abs(id - bottomId) < Math.Abs(id - topId))
		{
			id = bottomId;
		}
		else
		{
			id = topId;
		}
	}

	public static bool InvalidId(int id)
	{
		var data = TileObjectData.GetTileData(id, 0);
		return (data == null || DataHasNoAnchors(data)) && Main.tileFrameImportant[id] || Main.tileCut[id] || id == TileID.Cactus || id == TileID.Trees
			|| TileID.Sets.IsVine[id] || id < TileID.Count && !Main.tileSolid[id] || ModContent.GetModTile(id) is ModTile modTile && modTile.Mod.Name == "ModLoaderMod";
	}

	private static bool DataHasNoAnchors(TileObjectData data)
	{
		return data.AnchorBottom == AnchorData.Empty && data.AnchorTop == AnchorData.Empty && data.AnchorRight == AnchorData.Empty && data.AnchorLeft == AnchorData.Empty;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
	}

	public override void Update()
	{
		if (!BossSpawned && NPC.AnyNPCs(NPCID.HallowBoss))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.HallowBoss) && !ExitSpawned)
		{
			ExitSpawned = true;

			HashSet<Player> players = [];

			foreach (Player plr in Main.ActivePlayers)
			{
				if (!plr.dead)
				{
					players.Add(plr);
				}
			}

			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Vector2 position = Main.rand.Next([.. players]).Center - new Vector2(0, 60);
			Projectile.NewProjectile(src, position, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}
}
