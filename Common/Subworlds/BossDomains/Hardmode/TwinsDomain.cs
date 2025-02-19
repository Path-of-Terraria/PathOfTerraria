using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class TwinsDomain : BossDomainSubworld
{
	public override int Width => 800;
	public override int Height => 1400;
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength - 10000, true);

	internal static Point16 CircleCenter = Point16.Zero;

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain)];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (j > 200)
				{
					tile.HasTile = true;
					
					if (j > 800)
					{
						tile.TileType = TileID.Ebonstone;
					}
					else if (j > 400)
					{
						tile.TileType = TileID.LeadBrick;
					}
					else
					{
						tile.TileType = TileID.Dirt;
					}
				}
			}
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
	}

	public override void Update()
	{
		if (!BossSpawned)
		{
			
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.QueenSlimeBoss) && !ExitSpawned)
			{
				ExitSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				//Projectile.NewProjectile(src, ArenaPos.ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}
}
