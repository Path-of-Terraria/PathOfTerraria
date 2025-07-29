using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;
using PathOfTerraria.Content.Projectiles.Utility;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class EmpressDomain : BossDomainSubworld, IOverrideBiome
{
	public override int Width => 670;
	public override int Height => 600;
	public override (int time, bool isDay) ForceTime => (3500, false);

	private static Rectangle ArenaBounds = new();
	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static int Wave = 0;
	private static int WaitTime = 0;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain)];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = Height / 2;
		Main.worldSurface = Height - 50;
		Main.rockLayer = Height - 40;

		Point16 size = StructureTools.GetSize("Assets/Structures/EmpressDomain/Arena");
		Point16 pos = StructureTools.PlaceByOrigin("Assets/Structures/EmpressDomain/Arena", new Point16(Width / 2, Height / 2 - 5), new Vector2(0.5f, 0));

		ArenaBounds = new Rectangle(pos.X * 16, pos.Y * 16, size.X * 16, size.Y * 16);
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
		Wave = 0;
		WaitTime = 0;
	}

	public override void Update()
	{
		DoWaveFunctionality();

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

	private static void DoWaveFunctionality()
	{
		int count = 0;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			count++;
		}

		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.type == ModContent.ProjectileType<SpawnSymbols>())
			{
				count++;
			}
		}

		if (count <= 0)
		{
			WaitTime++;

			if (WaitTime > 20)
			{
				Wave++;
				WaitTime = 0;

				if (Wave == 1)
				{
					for (int i = 0; i < 8; ++i)
					{
						SpawnSpawner(NPCID.Pixie);
					}
				}
				else if (Wave == 2)
				{
					for (int i = 0; i < 8; ++i)
					{
						SpawnSpawner(i < 6 ? NPCID.Pixie : NPCID.Gastropod);
					}
				}
				else if (Wave == 3)
				{
					for (int i = 0; i < 8; ++i)
					{
						SpawnSpawner(i < 6 ? NPCID.Gastropod : NPCID.Unicorn);
					}
				}
				else if (Wave == 4)
				{
					for (int i = 0; i < 5; ++i)
					{
						SpawnSpawner(i < 2 ? NPCID.RainbowSlime : (i == 4 ? ModContent.NPCType<GreaterFairy>() : NPCID.Unicorn));
					}
				}
				else if (Wave == 5)
				{
					SpawnSpawner(NPCID.QueenSlimeBoss, new Vector2(Main.spawnTileX, Main.spawnTileY) * 16);
				}
				else if (Wave == 6)
				{
					for (int i = 0; i < 3; ++i)
					{
						SpawnSpawner(ModContent.NPCType<GreaterFairy>());
					}
				}
				else if (Wave == 7)
				{
					for (int i = 0; i < 3; ++i)
					{
						SpawnSpawner(ModContent.NPCType<Prismatism>());
					}
				}
				else if (Wave == 8)
				{
					for (int i = 0; i < 6; ++i)
					{
						SpawnSpawner(i < 3 ? ModContent.NPCType<GreaterFairy>() : ModContent.NPCType<Prismatism>());
					}
				}
				else if (Wave == 9)
				{
					for (int i = 0; i < 12; ++i)
					{
						SpawnSpawner(i switch
						{
							< 3 => ModContent.NPCType<GreaterFairy>(),
							< 6 => ModContent.NPCType<Prismatism>(),
							< 9 => NPCID.RainbowSlime,
							_ => NPCID.Unicorn
						});
					}
				}
				else if (Wave == 10)
				{
					SpawnSpawner(NPCID.HallowBoss, new Vector2(Main.spawnTileX, Main.spawnTileY - 10) * 16);
				}
			}
		}
	}

	private static void SpawnSpawner(int type, Vector2? forcePos = null)
	{
		var src = new EntitySource_SpawnNPC();
		int proj = ModContent.ProjectileType<SpawnSymbols>();
		Vector2 pos = forcePos ?? RandomArenaPosition();
		int newProj = Projectile.NewProjectile(src, pos, Vector2.Zero, proj, 0, 0, Main.myPlayer, type, Main.rand.NextFloat(240));

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, newProj);
		}
	}

	private static Vector2 RandomArenaPosition()
	{
		int halfX = Main.maxTilesX / 2;
		return new(Main.rand.Next(halfX - 20, halfX + 20) * 16, Main.rand.Next(ArenaBounds.Top - 50, ArenaBounds.Top + 200));
	}

	public void OverrideBiome()
	{
		Main.LocalPlayer.ZoneHallow = true;
		Main.newMusic = MusicID.TheHallow;
		Main.curMusic = MusicID.TheHallow;
		Main.bgStyle = SurfaceBackgroundID.Hallow;
	}
}
