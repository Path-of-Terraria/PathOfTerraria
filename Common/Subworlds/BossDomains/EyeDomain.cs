using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using SubworldLibrary;
using Terraria.Enums;
using Terraria.Localization;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class EyeDomain : BossDomainSubworld
{
	public const int ArenaX = 620;

	public override int Width => 800;
	public override int Height => 280;
	public override (int time, bool isDay) ForceTime => ((int)(Main.nightLength / 2.0), false);

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenSurface),
		new PassLegacy("Grass", (progress, _) => PlaceGrassAndDecor(progress, true, Mod, out Arena))];

	public static void PlaceGrassAndDecor(GenerationProgress progress, bool includeFleshStuff, Mod mod, out Rectangle arena)
	{
		arena = Rectangle.Empty;
		Dictionary<Point16, OpenFlags> tiles = [];
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 80; j < Main.maxTilesY - 50; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || tile.TileType != TileID.Dirt || tiles.ContainsKey(new Point16(i, j)))
				{
					continue;
				}

				OpenFlags flags = OpenExtensions.GetOpenings(i, j);

				if (flags == OpenFlags.None)
				{
					continue;
				}

				tiles.Add(new Point16(i, j), flags);
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		int arenaY = 0;
		HashSet<Point16> grasses = [];

		foreach ((Point16 position, OpenFlags tile) in tiles)
		{
			TrySpreadGrassOnTile(tile, position, grasses, includeFleshStuff);

			if (position.X == ArenaX && Main.tile[position].HasTile && Main.tile[position].TileType == TileID.Grass)
			{
				arenaY = position.Y;
			}
		}

		int structureX = 0;

		foreach (Point16 position in grasses)
		{
			if (includeFleshStuff && Main.tile[position].TileType == TileID.FleshBlock)
			{
				if (position.X - structureX > 60 && position.X < ArenaX - 20 && WorldGen.genRand.NextBool(20) && !WorldGen.SolidOrSlopedTile(position.X, position.Y - 1))
				{
					int typeId = WorldGen.genRand.Next(3);

					string type = "Assets/Structures/EoCDomain/" + typeId switch
					{
						0 => "Graveyard",
						1 => "Flesh",
						2 => "Shrine",
						_ => "Wound"
					};

					type += WorldGen.genRand.Next(typeId switch
					{
						0 or 2 => 3,
						1 => 2,
						_ => 4
					});

					var pos = new Point16(position.X, position.Y + 4);

					if (typeId == 0)
					{
						pos = new Point16(position.X, position.Y + 1);
					}

					Point16 size = StructureTools.GetSize(type);
					pos = StructureTools.PlaceByOrigin(type, pos, new Vector2(0.5f, 1f), null, true);
					structureX = pos.X;
				}

				if (WorldGen.genRand.NextBool(20))
				{
					WorldGen.PlaceObject(position.X, position.Y - 1, ModContent.TileType<EmbeddedEye>(), true, WorldGen.genRand.Next(2));
				}

				continue;
			}

			Decoration.OnPurityGrass(position);
		}

		if (includeFleshStuff)
		{
			var dims = new Point16();
			StructureHelper.Generator.GetDimensions("Assets/Structures/EyeArena", mod, ref dims);
			StructureHelper.Generator.GenerateStructure("Assets/Structures/EyeArena", new Point16(ArenaX, arenaY - 27), mod);
			arena = new Rectangle(ArenaX * 16, (arenaY + 2) * 16, dims.X * 16, (dims.Y - 2) * 16);
		}

		CheckForSigns(new Point16(20, 20), new Point16(Main.maxTilesX - 60, Main.maxTilesY - 60));
	}

	private static void CheckForSigns(Point16 pos, Point16 size)
	{
		for (int i = pos.X; i < pos.X + size.X; ++i)
		{
			for (int j = pos.Y; j < pos.Y + size.Y; ++j)
			{
				if (!WorldGen.InWorld(i, j))
				{
					continue;
				}

				WorldGen.TileFrame(i, j);

				int sign = Sign.ReadSign(i, j, true);

				if (sign != -1)
				{
					Sign.TextSign(sign, Language.GetText("Mods.PathOfTerraria.Generation.EyeSign." + WorldGen.genRand.Next(4))
						.WithFormatArgs(Language.GetText("Mods.PathOfTerraria.Generation.EyeSign.Names." + WorldGen.genRand.Next(9)).Value).Value);
				}
			}
		}
	}

	private static void TrySpreadGrassOnTile(OpenFlags adjacencies, Point16 position, HashSet<Point16> grasses, bool includeFlesh)
	{
		Tile tile = Main.tile[position];

		if (adjacencies == OpenFlags.Above)
		{
			tile.TileType = TileID.Grass;

			if (includeFlesh && !StepX(position.X) && position.X > 150 && WorldGen.genRand.NextBool(15))
			{
				WorldGen.TileRunner(position.X, position.Y, WorldGen.genRand.NextFloat(12, 26), WorldGen.genRand.Next(12, 40), TileID.FleshBlock);
			}
			else
			{
				grasses.Add(position);
			}
		}
	}

	private void GenSurface(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = 80;
		Main.spawnTileY = 210;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		float baseY = 220;
		FastNoiseLite noise = GetGenNoise();
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			if (StepX(x))
			{
				baseY -= WorldGen.genRand.NextFloat(0.8f);
			}

			float useY = baseY + noise.GetNoise(x, 0) * 4;

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				Tile tile = Main.tile[x, y];
				tile.HasTile = true;
				tile.TileType = TileID.Dirt;
			}

			progress.Value = (float)x / Main.maxTilesX;
		}
	}

	private static bool StepX(int x)
	{
		return x is > 100 and < 150 || x is > 300 and < 400 || x is > 500 and < 550;
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.01f);
		return noise;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ReadyToExit = false;
	}

	public override bool GetLight(Tile tile, int x, int y, ref FastRandom rand, ref Vector3 color)
	{
		if (!Main.tile[x, y].HasTile)
		{
			color = Vector3.Max(color, new Vector3(0.2f, 0.2f, 0.2f));
		}

		return true;
	}

	public override void Update()
	{
		TileEntity.UpdateStart();

		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();
		Main.moonPhase = (int)MoonPhase.Full;

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
				WorldGen.PlaceTile(Arena.X / 16 + i + 4, Arena.Y / 16 - 3, TileID.FleshBlock, true, true);
			}

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X - 130, Arena.Center.Y - 400, NPCID.EyeofCthulhu);
			BossSpawned = true;

			Main.spawnTileX = Arena.Center.X / 16;
			Main.spawnTileY = Arena.Center.Y / 16;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendTileSquare(-1, Arena.X / 16 + 4, Arena.Y / 16 - 3, 20, 1);
				NetMessage.SendData(MessageID.WorldData);
			}
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.EyeofCthulhu) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(-130, -300);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.EyeofCthulhu);
			ReadyToExit = true;
		}
	}

	public class EyeSceneEffect : ModSceneEffect
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
		public override int Music => MusicID.Eerie;

		public override bool IsSceneEffectActive(Player player)
		{
			return SubworldSystem.Current is EyeDomain;
		}
	}
}