using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.Systems.DisableBuilding;
using Terraria.Enums;
using Terraria.Localization;
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using PathOfTerraria.Common.Subworlds.Passes;
using SubworldLibrary;
using Terraria.Utilities;
using System.Drawing.Printing;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class DeerclopsDomain : BossDomainSubworld
{
	public static int Surface => 200;

	public override int Width => 800;
	public override int Height => 800;

	internal static float LightMultiplier = 0;

	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new FlatWorldPass(Surface, true, GetSurfaceNoise()), 
		new PassLegacy("Tunnels", Tunnels)];

	public override void Load()
	{
		On_Lighting.AddLight_int_int_float_float_float += HijackAddLight;
		On_Player.ItemCheck += SoftenPlayerLight;
	}

	private static FastNoiseLite GetSurfaceNoise()
	{
		var noise = new FastNoiseLite();
		noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
		noise.SetFrequency(0.04f);
		return noise;
	}

	private void Tunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		FastNoiseLite noise = GetSurfaceNoise();
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = (int)(Height * 0.7f);

		StructureTools.PlaceByOrigin("Assets/Structures/DeerclopsDomain/Start_0", new Point16(Main.spawnTileX, Main.spawnTileY), new(0.5f));

		int firstTunnelXStart = Main.spawnTileX + WorldGen.genRand.Next(40, 80) * (WorldGen.genRand.NextBool() ? -1 : 1);
		Vector2[] points = Tunnel.GeneratePoints([new(Main.spawnTileX, Main.spawnTileY), new(firstTunnelXStart, Main.spawnTileY - 60)], 6, 4);
		DigThrough(points, noise);
	}

	private static void DigThrough(Vector2[] points, FastNoiseLite noise)
	{
		foreach (Vector2 item in points)
		{
			float mul = 1f + MathF.Abs(noise.GetNoise(item.X, item.Y)) * 1.2f;
			Digging.CircleOpening(item, 5 * mul);
			Digging.CircleOpening(item, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(3, 5))
			{
				WorldGen.digTunnel(item.X, item.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(1, 8) * mul));
			}
		}
	}

	private void SoftenPlayerLight(On_Player.orig_ItemCheck orig, Player self)
	{
		LightMultiplier = 0.1f;
		orig(self);
		LightMultiplier = 0;
	}

	private void HijackAddLight(On_Lighting.orig_AddLight_int_int_float_float_float orig, int i, int j, float r, float g, float b)
	{
		if (SubworldSystem.Current is DeerclopsDomain && j > Surface + 10)
		{
			(r, g, b) = (r * LightMultiplier, g * LightMultiplier, b * LightMultiplier);
		}

		orig(i, j, r, g, b);
	}

	public override bool GetLight(Tile tile, int x, int y, ref FastRandom rand, ref Vector3 color)
	{
		if (y > Surface)
		{
			float mul = 0;

			if (y < Surface + 10)
			{
				mul = (y - Surface) / 10f;
			}

			color *= mul;
			return true;
		}

		return false;
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
	}

	public override void Update()
	{
		Liquid.UpdateLiquid();
		Wiring.UpdateMech();

		TileEntity.UpdateStart();
		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		Main.dayTime = true;
		Main.time = Main.dayLength / 2;
		Main.moonPhase = (int)MoonPhase.Full;

		foreach (Player player in Main.ActivePlayers)
		{
			player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;
		}

		if (!BossSpawned && NPC.AnyNPCs(NPCID.QueenBee))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.QueenBee) && !ReadyToExit)
		{
			Vector2 pos = new Vector2(Width / 2, Height / 4 - 8) * 16;
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.QueenBee);
			ReadyToExit = true;
		}
	}

	private class DeerclopsDomainPlayer : ModPlayer
	{
		private int LightTime = 0;

		public override void UpdateEquips()
		{
			Point16 center = Player.Center.ToTileCoordinates16();

			if (SubworldSystem.Current is not DeerclopsDomain || Lighting.Brightness(center.X, center.Y) > 0.9f)
			{
				LightTime--;
			}
			else
			{
				LightTime++;

				if (LightTime > 5 * 60 && LightTime % (3 * 60) == 0)
				{
					Vector2 projPos = Player.Center + Main.rand.NextVector2CircularEdge(160, 160);
					Projectile.RandomizeInsanityShadowFor(Main.player[Player.whoAmI], true, out Vector2 spawnPosition, out Vector2 vel, out float ai, out float ai2);
					Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), spawnPosition, vel, ProjectileID.InsanityShadowHostile, 60, 6, Main.myPlayer, ai, ai2);
				}
			}

			if (LightTime <= 0)
			{
				LightTime = 0;
			}	
		}
	}
}