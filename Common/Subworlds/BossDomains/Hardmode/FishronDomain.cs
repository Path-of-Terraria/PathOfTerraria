using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class FishronDomain : BossDomainSubworld, IOverrideBiome
{
	public const int FloorY = 180;

	public override int Width => 1300;
	public override int Height => 400;
	public override (int time, bool isDay) ForceTime => (4600, true);

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static Rectangle Arena = Rectangle.Empty;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new FlatWorldPass(FloorY, true, FlatNoise(), TileID.Mud, WallID.MushroomUnsafe, 18)];

	private static FastNoiseLite FlatNoise()
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.006f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

		noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);

		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
		noise.SetDomainWarpAmp(-900);
		return noise;
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

		if (!BossSpawned)
		{
			bool canSpawn = Main.CurrentFrameFlags.ActivePlayersCount > 0;
			HashSet<int> who = [];

			if (canSpawn)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (!Arena.Intersects(player.Hitbox))
					{
						canSpawn = false;
						break;
					}
					else
					{
						who.Add(player.whoAmI);
					}
				}
			}

			if (canSpawn && Main.CurrentFrameFlags.ActivePlayersCount > 0 && who.Count > 0)
			{
				int plr = Main.rand.Next([.. who]);
				IEntitySource src = Entity.GetSource_NaturalSpawn();
				
				int npc = NPC.NewNPC(src, (int)Arena.Center().X, (int)Arena.Center().Y - 25, NPCID.SkeletronPrime);
				Main.npc[npc].GetGlobalNPC<ArenaEnemyNPC>().Arena = true;

				Main.spawnTileX = (int)Arena.Center().X / 16;
				Main.spawnTileY = (int)Arena.Center().Y / 16;

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
					NetMessage.SendTileSquare(-1, Arena.X / 16 + 72, Arena.Y / 16, 20, 1);
				}

				BossSpawned = true;
			}
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.SkeletronPrime) && !ExitSpawned)
			{
				ExitSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				Projectile.NewProjectile(src, Arena.Center() - new Vector2(0, 60), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}

	public void OverrideBiome()
	{
		Main.LocalPlayer.ZoneCorrupt = true;
		Main.newMusic = MusicID.Corruption;
		Main.curMusic = MusicID.Corruption;
	}
}
