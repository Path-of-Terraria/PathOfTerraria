using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.Localization;
using SubworldLibrary;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class EaterDomain : BossDomainSubworld
{
	public override int Width => 800;
	public override int Height => 1000;
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<WeakMalaise>(), ModContent.TileType<TeethSpikes>()];
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength - 1800, true);

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	private HashSet<Point16> DemonitePositions = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenTerrain),
		new PassLegacy("Arena", SpawnArena),
		new PassLegacy("Grasses", PlaceGrassAndDecor)];

	private void PlaceGrassAndDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		Dictionary<Point16, OpenFlags> tiles = [];
		HashSet<Point16> empty = [];

		for (int i = 6; i < Main.maxTilesX - 6; ++i)
		{
			for (int j = 80; j < Main.maxTilesY - 60; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || tiles.ContainsKey(new Point16(i, j)))
				{
					if (j > 200 && j < Height - 200)
					{
						empty.Add(new Point16(i, j));
					}

					continue;
				}

				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (flags == OpenFlags.None)
				{
					continue;
				}

				tiles.Add(new Point16(i, j), flags);
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (Point16 pos in empty)
		{
			if (WorldGen.genRand.NextBool(2200))
			{
				WorldGen.PlaceObject(pos.X + 1, pos.Y + 1, (ushort)ModContent.TileType<CorruptSacks>(), true, WorldGen.genRand.Next(3));
			}
		}

		HashSet<Point16> boneSpikes = [];
		HashSet<Point16> eggs = [];

		foreach (KeyValuePair<Point16, OpenFlags> item in tiles)
		{
			if (item.Key.Y > 260 && item.Key.Y < Height - 200)
			{
				if (WorldGen.genRand.NextBool(200))
				{
					boneSpikes.Add(item.Key);
				}
				else if (WorldGen.genRand.NextBool(1200) && (item.Value == OpenFlags.Above || item.Value == OpenFlags.Below) && item.Value != (OpenFlags.Above | OpenFlags.Below))
				{
					eggs.Add(item.Key);
				}
			}
		}

		List<Point16> newSpikes = [];

		foreach (Point16 position in boneSpikes)
		{
			tiles.Remove(position);
			int distance = WorldGen.genRand.Next(12, 22);

			foreach (KeyValuePair<Point16, OpenFlags> tile in tiles)
			{
				if (Vector2.DistanceSquared(tile.Key.ToVector2(), position.ToVector2()) < distance)
				{
					newSpikes.Add(tile.Key);
				}
			}
		}

		foreach (Point16 item in newSpikes)
		{
			tiles.Remove(item);
			boneSpikes.Add(item);
		}

		foreach (Point16 position in boneSpikes)
		{
			WorldGen.PlaceTile(position.X, position.Y, ModContent.TileType<TeethSpikes>(), true, true);
		}

		foreach (Point16 position in eggs)
		{
			OpenFlags flags = OpenExtensions.GetOpenings(position.X, position.Y, false, false);
			float angle = GetAngleFromFlags(flags);
			Vector2 angleOffset = -new Vector2(0, -10).RotatedBy(angle);
			var placePos = new Point16(position.X + (int)angleOffset.X, position.Y + (int)angleOffset.Y);

			StructureTools.PlaceByOrigin("Assets/Structures/EaterDomain/Egg" + WorldGen.genRand.Next(3), placePos, new Vector2(0.5f, 0.5f), null);
		}

		HashSet<Point16> grasses = [];

		foreach (KeyValuePair<Point16, OpenFlags> item in tiles)
		{
			Tile tile = Main.tile[item.Key];

			if (tile.TileType == TileID.Dirt)
			{
				tile.TileType = TileID.CorruptGrass;
				grasses.Add(item.Key);
			}

			PlaceDecor(item.Key, item.Value);
		}

		foreach (Point16 pos in DemonitePositions)
		{
			ushort type = WorldGen.genRand.NextBool(5) ? TileID.Gold : TileID.Demonite;
			WorldGen.TileRunner(pos.X, pos.Y, WorldGen.genRand.Next(6, 16), WorldGen.genRand.Next(4, 20), type);
		}

		int lastStrX = 0;

		foreach (Point16 pos in grasses)
		{
			if (Math.Abs(pos.X - lastStrX) > 50 && Math.Abs(pos.X - 400) > 210 && WorldGen.genRand.NextBool(90))
			{
				int typeId = WorldGen.genRand.Next(2);
				string type = "Assets/Structures/EaterDomain/" + typeId switch
				{
					0 => "Ruin",
					_ => "Graveyard"
				};

				type += WorldGen.genRand.Next(3) + WorldGen.genRand.Next(typeId + 1);
				StructureTools.PlaceByOrigin(type, new Point16(pos.X, pos.Y + 1), new Vector2(0.5f, 1f));
				lastStrX = pos.X;
			}
		}

		CheckForSigns(new Point16(10, 100), new Point16(Main.maxTilesX - 20, Main.maxTilesY - 120));
	}

	private static void CheckForSigns(Point16 pos, Point16 size)
	{
		for (int i = pos.X; i < pos.X + size.X; ++i)
		{
			for (int j = pos.Y; j < pos.Y + size.Y; ++j)
			{
				WorldGen.TileFrame(i, j);

				int sign = Sign.ReadSign(i, j, true);

				if (sign != -1)
				{
					Sign.TextSign(sign, Language.GetText("Mods.PathOfTerraria.Generation.EaterSign." + WorldGen.genRand.Next(4))
						.WithFormatArgs(Language.GetText("Mods.PathOfTerraria.Generation.EaterSign.Names." + WorldGen.genRand.Next(9)).Value).Value);
				}
			}
		}
	}

	private float GetAngleFromFlags(OpenFlags flags)
	{
		List<float> angles = [];

		if (flags == OpenFlags.None)
		{
			return float.NaN;
		}

		if (flags.HasFlag(OpenFlags.Above))
		{
			angles.Add(0);
		}

		if (flags.HasFlag(OpenFlags.Below))
		{
			angles.Add(MathHelper.Pi);
		}

		float angle = 0;

		foreach (float ang in angles)
		{
			angle += ang;
		}

		return angle / angles.Count;
	}

	private static void SpawnBoneSpikes(Point16 position)
	{
		WorldGen.TileRunner(position.X, position.Y, WorldGen.genRand.Next(6, 16), WorldGen.genRand.Next(4, 20), ModContent.TileType<TeethSpikes>());
	}

	private static void PlaceDecor(Point16 position, OpenFlags flags)
	{
		if (WorldGen.genRand.NextBool(8))
		{
			Tile.SmoothSlope(position.X, position.Y, true);
			return;
		}

		if (CanPlaceGraveyard(position) && WorldGen.genRand.NextBool(12) && position.Y > 300)
		{
			StructureTools.PlaceByOrigin("Assets/Structures/EaterDomain/UGGraveyard" + WorldGen.genRand.Next(3), position, new Vector2(0f, 1f), null, true);
			return;
		}

		Tile tile = Main.tile[position];

		if (flags.HasFlag(OpenFlags.Above))
		{
			if (tile.TileType == TileID.CorruptGrass)
			{
				if (!WorldGen.genRand.NextBool(3))
				{
					WorldGen.PlaceTile(position.X, position.Y - 1, TileID.CorruptPlants, true);
				}
				else if (WorldGen.genRand.NextBool(6))
				{
					WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);

					if (!WorldGen.GrowTree(position.X, position.Y - 1))
					{
						WorldGen.KillTile(position.X, position.Y - 1);
					}
				}
			}
			else if (tile.TileType == TileID.Ebonstone)
			{
				if (WorldGen.genRand.NextBool(4))
				{
					WorldGen.PlaceSmallPile(position.X, position.Y - 1, WorldGen.genRand.Next(12, 28), 0);
				}
				else if (WorldGen.genRand.NextBool(3))
				{
					GenPlacement.PlaceStalagmite(position.X, position.Y - 1, Main.rand.NextBool(4), 5, null);
				}
			}
		}
		
		if (flags.HasFlag(OpenFlags.Below))
		{
			if (tile.TileType == TileID.Ebonstone)
			{
				if (WorldGen.genRand.NextBool(3))
				{
					GenPlacement.PlaceStalactite(position.X, position.Y, WorldGen.genRand.NextBool(4), 5);
				}
			}
		}
	}

	private static bool CanPlaceGraveyard(Point16 position)
	{
		for (int i = position.X; i < position.X + 10; ++i)
		{
			for (int j = position.Y; j > position.Y - 5; --j)
			{
				if (j == position.Y && !WorldGen.SolidTile(i, j))
				{
					return false;
				}

				if (j < position.Y && WorldGen.SolidTile(i, j))
				{
					return false;
				}
			}
		}

		return true;
	}

	private void SpawnArena(GenerationProgress progress, GameConfiguration configuration)
	{
		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/EaterArena", Mod, ref size);
		var position = new Point16(400 - size.X / 2, Height - 150 - size.Y / 2);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/EaterArena", position, Mod);

		Arena = new Rectangle(position.X * 16, position.Y * 16, size.X * 16, size.Y * 16);
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = WorldGen.genRand.NextBool() ? 80 : Main.maxTilesX - 80;
		Main.spawnTileY = 110;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		float baseY = 120;

		FastNoiseLite noise = GetGenNoise();
		DemonitePositions = [];
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			float noiseOffset = noise.GetNoise(x, 0) * 3;
			float useY = baseY + noiseOffset;
			int difference = Math.Abs(x - 400);

			if (difference < 200)
			{
				useY += (200 - difference) / 4;
			}

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				WorldGen.PlaceTile(x, y, y > 400 + noiseOffset ? TileID.Ebonstone : TileID.Dirt);

				if (y > useY + 4)
				{
					WorldGen.PlaceWall(x, y, WallID.EbonstoneUnsafe, true);
				}

				if (y > 400 + noiseOffset && WorldGen.genRand.NextBool(1600))
				{
					DemonitePositions.Add(new Point16(x, y));
				}
			}

			progress.Value = (float)x / Main.maxTilesX;
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");
		progress.Value = 0;

		// Chasm one
		List<Vector2> breakthroughs = [];
		DigChasm(noise, Tunnel.GeneratePoints(GenerateWindingTunnel(400, baseY, 400, baseY + 220), 26, 6), null);

		// Tunnel one
		progress.Value = 0.2f;
		Vector2[] horizontalPoints = Tunnel.GeneratePoints(GenerateHorizontalTunnel(60, baseY + 200, 740, baseY + 200), 20, 6, 0.3f);
		DigChasm(noise, horizontalPoints, (100, 700, 40), 2.8f, true);
		GetRandomPoint(breakthroughs, horizontalPoints);

		// Chasm two
		progress.Value = 0.4f;
		Vector2[] chasm = Tunnel.GeneratePoints(GenerateWindingTunnel((int)breakthroughs[0].X, (int)breakthroughs[0].Y - 20, 400, (int)breakthroughs[0].Y + 200), 26, 6);
		DigChasm(noise, chasm, null);

		// Tunnel two
		progress.Value = 0.6f;
		horizontalPoints = Tunnel.GeneratePoints(GenerateHorizontalTunnel(60, (int)breakthroughs[0].Y + 200, 740, (int)breakthroughs[0].Y + 200), 15, 6, 0.3f);
		DigChasm(noise, horizontalPoints, (100, 700, 40), 2.4f, true);
		breakthroughs.Add(WorldGen.genRand.Next(horizontalPoints));
		breakthroughs[0] = chasm[chasm.Length / 5];

		// Last chasm
		progress.Value = 0.8f;
		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/EaterArena", Mod, ref size);
		chasm = Tunnel.GeneratePoints(GenerateWindingTunnel((int)breakthroughs[1].X, (int)breakthroughs[1].Y - 20, 400, Height - 240, 0.2f), 12, 6);
		breakthroughs[1] = chasm[chasm.Length / 8];
		DigChasm(noise, chasm, null, 2.4f);
		DigChasm(noise, Tunnel.GeneratePoints(GenerateWindingTunnel(400, Height - 270, 400, Height - 120, 0.1f), 12, 10), null, 2f);

		progress.Value = 1f;
		// Opening before the arena
		WorldGen.digTunnel(403, Height - 200, 0, 0, 20, 20);

		foreach (Vector2 item in breakthroughs)
		{
			WorldGen.TileRunner((int)item.X, (int)item.Y, 40, WorldGen.genRand.Next(20, 40), ModContent.TileType<WeakMalaise>(), true);
		}
	}

	private static void GetRandomPoint(List<Vector2> breakthroughs, Vector2[] horizontalPoints)
	{
		Vector2 position = WorldGen.genRand.Next(horizontalPoints);

		while (Math.Abs(position.X - 400) < 100)
		{
			position = WorldGen.genRand.Next(horizontalPoints);
		}

		breakthroughs.Add(position);
	}

	private Vector2[] GenerateHorizontalTunnel(int x, float y, int targetX, float targetY)
	{
		const int MaxSteps = 8;

		List<Vector2> positions = [new Vector2(x, y)];

		for (int i = 1; i < MaxSteps - 1; ++i)
		{
			var lerp = Vector2.Lerp(new Vector2(x, y), new Vector2(targetX, targetY), i / (MaxSteps - 1f));
			lerp += new Vector2(0, WorldGen.genRand.Next(-2, 2));

			positions.Add(lerp);
		}

		positions.Add(new Vector2(targetX, targetY));
		return [.. positions];
	}

	private static void DigChasm(FastNoiseLite noise, Vector2[] positions, (int baseX, int endX, int fadeAway)? smoothInOut, float sizeMul = 1.2f, bool digTunnel = true)
	{
		foreach (Vector2 item in positions)
		{
			float mul = MathHelper.Max(1f + MathF.Abs(noise.GetNoise(item.X, item.Y)) * sizeMul, 0.5f);

			if (smoothInOut.HasValue)
			{
				int fade = smoothInOut.Value.fadeAway;

				if (item.X < smoothInOut.Value.baseX)
				{
					mul *= (fade - MathF.Min(smoothInOut.Value.baseX - item.X, fade)) / (float)fade;
				}
				else if (item.X > smoothInOut.Value.endX)
				{
					mul *= (fade - MathF.Min(item.X - smoothInOut.Value.endX, fade)) / (float)fade;
				}
			}

			Digging.CircleOpening(item, 5 * mul);
			Digging.CircleOpening(item, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(8))
			{
				Digging.WallCircleOpening(item, WorldGen.genRand.Next(4, 7));
			}

			if (digTunnel && WorldGen.genRand.NextBool(3, 5))
			{
				WorldGen.digTunnel(item.X, item.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(1, 8) * mul));
			}

			WorldGen.TileRunner((int)item.X, (int)item.Y, 120, 8, TileID.Ebonstone, false, 0, 0, false, true);
		}
	}

	private Vector2[] GenerateWindingTunnel(int x, float y, int targetX, float targetY, float windingMultiplier = 1f)
	{
		const int MaxSteps = 3;

		List<Vector2> positions = [new Vector2(x, y)];

		for (int i = 0; i < MaxSteps - 1; ++i)
		{
			var lerp = Vector2.Lerp(new Vector2(x, y), new Vector2(targetX, targetY), i / (MaxSteps - 1f));
			lerp += new Vector2((int)(WorldGen.genRand.Next(-100, 100) * windingMultiplier), (int)(WorldGen.genRand.Next(-20, 20) * windingMultiplier));
			positions.Add(lerp);
		}

		positions.Add(new Vector2(targetX, targetY));
		return [.. positions];
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.008f);
		return noise;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ReadyToExit = false;
	}

	public override void Update()
	{
		bool allInArena = Main.CurrentFrameFlags.ActivePlayersCount > 0;

		foreach (Player player in Main.ActivePlayers)
		{
			if (allInArena && !Arena.Intersects(player.Hitbox))
			{
				allInArena = false;
			}
		}

		if (!BossSpawned && allInArena)
		{
			for (int i = 0; i < 20; ++i)
			{
				WorldGen.PlaceTile(Arena.X / 16 + i + 72, Arena.Y / 16, TileID.Ebonstone, true, true);
			}

			int headOne = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X + 1400, Arena.Center.Y - 0, NPCID.EaterofWorldsHead, 1);
			int headTwo = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X - 1400, Arena.Center.Y - 0, NPCID.EaterofWorldsHead, 1);

			Main.spawnTileX = Arena.Center.X / 16;
			Main.spawnTileY = Arena.Center.Y / 16;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
				NetMessage.SendTileSquare(-1, Arena.X / 16 + 72, Arena.Y / 16, 20, 1);
			}

			BossSpawned = true;
		}

		if (BossSpawned && NoEoW() && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(0, 240);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.EaterofWorldsHead);
			ReadyToExit = true;
		}
	}

	private static bool NoEoW()
	{
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsTail)
			{
				return false;
			}
		}

		return true;
	}
}

/// <summary>
/// Used solely to reduce the number of segments per Eater from 67/72 to 30/34 in order to account for there being two Eaters in <see cref="EaterDomain"/>.
/// </summary>
public class EaterEdit : ILoadable
{
	public void Load(Mod mod)
	{
		IL_NPC.AI_006_Worms += HijackSpawnBody;
	}

	private void HijackSpawnBody(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchCall<NPC>(nameof(NPC.GetEaterOfWorldsSegmentsCount))))
		{
			return;
		}

		if (!c.TryGotoNext(x => x.MatchCall<NPC>(nameof(NPC.NewNPC))))
		{
			return;
		}

		c.Emit(OpCodes.Ldarg_0);
		c.EmitDelegate((NPC npc) =>
		{
			if (SubworldSystem.Current is EaterDomain)
			{
				npc.ai[2] = Main.expertMode ? 28 : 24;
			}
		});
	}

	public void Unload() { }
}