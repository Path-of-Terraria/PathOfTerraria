using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class CultistDomain : BossDomainSubworld
{
	public override int Width => 1400;
	public override int Height => 800;
	public override (int time, bool isDay) ForceTime => ((int)(Main.dayLength * 0.95), true);

	public static Point16 BulbPosition = new();
	public static int BulbsBroken = 0;

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new FlatWorldPass(500, tileType: TileID.BlueDungeonBrick)];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		BulbsBroken = 0;
		BossSpawned = false;
		ExitSpawned = false;

		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{

			}

			progress.Value = (float)i / Main.maxTilesX;
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
		Wiring.UpdateMech();
		TileEntity.UpdateStart();

		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		if (!BossSpawned && NPC.AnyNPCs(NPCID.Plantera))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.Plantera) && !ExitSpawned)
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
