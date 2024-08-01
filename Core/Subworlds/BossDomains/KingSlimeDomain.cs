using PathOfTerraria.Core.Subworlds.Passes;
using PathOfTerraria.Core.Systems.DisableBuilding;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Core.Subworlds;

public class KingSlimeDomain : MappingWorld
{
	public override int Width => 500;
	public override int Height => 600;

	public Point16 ArenaEntrance = Point16.Zero;
	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;

	public override List<GenPass> Tasks => [new FlatWorldPass(0), new PassLegacy("Spawn Prefab", SpawnKingArena), new PassLegacy("Tunnel", TunnelGen)];

	private void TunnelGen(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = 250;
		Main.spawnTileY = 500;

		WorldGen.digTunnel(250, 500, 0, 0, 230, 15, false);
	}

	private void SpawnKingArena(GenerationProgress progress, GameConfiguration configuration)
	{
		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Data/Structures/KingSlimeArena", Mod, ref size);
		StructureHelper.Generator.GenerateStructure("Data/Structures/KingSlimeArena", new Point16(250 - size.X / 2, 120 - size.Y / 2), Mod);

		Arena = new Rectangle((250 - size.X / 2) * 16, (120 - size.Y / 2) * 16, size.X * 16, (size.Y - 4) * 16);
		ArenaEntrance = new Point16(250, 120 + size.Y / 2);
	}

	public override void Update()
	{
		bool allInArena = true;

		foreach (Player player in Main.ActivePlayers)
		{
			player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;

			if (allInArena && !Arena.Intersects(player.Hitbox))
			{
				allInArena = false;
			}
		}

		if (!BossSpawned && allInArena)
		{
			for (int i = -1; i < 11; ++i)
			{
				WorldGen.PlaceTile(ArenaEntrance.X + i, ArenaEntrance.Y - 1, TileID.SlimeBlock, true, true);
			}

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y, NPCID.KingSlime);
			BossSpawned = true;
		}
	}
}