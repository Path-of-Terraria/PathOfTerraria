﻿using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal partial class Grovetender : ModNPC
{
	public const int MaxRunestoneWait = 60 * 4;

	private void InitPoweredRunestones()
	{
		Point16 center = NPC.Center.ToTileCoordinates16();

		for (int i = center.X - 100; i < center.X + 100; ++i)
		{
			for (int j = center.Y - 100; j < center.Y + 100; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == ModContent.TileType<PoweredRunestone>())
				{
					PoweredRunestonePositions.Add(new Point16(i, j), 0);
				}
			}
		}
	}

	private void BoulderThrowBehaviour()
	{
		Timer++;
		NPC.TargetClosest(false);

		if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Vector2 pos = NPC.Center + new Vector2(Main.rand.NextFloat(-300, 300), 400);
			int type = ModContent.ProjectileType<GroveBoulder>();
			int count = SecondPhase ? 3 : 1;

			for (int i = 0; i < count; ++i)
			{
				_controlledWhoAmI.Add(Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, type, ModeUtils.ProjectileDamage(80), 8, Main.myPlayer));
			}
		}
		else if (Timer > 1 && Timer < 110)
		{
			int count = 0;

			foreach (int who in _controlledWhoAmI)
			{
				Projectile proj = Main.projectile[who];
				Vector2 targetPos = NPC.Center - new Vector2(0, 300);

				if (_controlledWhoAmI.Count > 1)
				{
					targetPos += new Vector2(60, 0).RotatedBy(count / (float)_controlledWhoAmI.Count * MathHelper.TwoPi + Timer * 0.02f);
				}

				if (count == 0)
				{
					if (Timer > 90 && Timer < 100)
					{
						targetPos += proj.DirectionFrom(Target.Center) * 340 * ((Timer - 80) / 10f);
					}
					else if (Timer >= 100)
					{
						targetPos += proj.DirectionFrom(Target.Center) * 340 * (1 - (Timer - 90) / 10f);
					}

					SpawnDust(NPC.Center - GetEyeOffset((EyeID)Main.rand.Next(2), true) + new Vector2(8), false);
				}

				proj.Center = Vector2.Lerp(proj.Center, targetPos, 0.06f);
				proj.velocity = Vector2.Zero;
				(proj.ModProjectile as GroveBoulder).Controlled = true;

				count++;
			}
		}
		else if (Timer == 110)
		{
			int who = _controlledWhoAmI.First();
			Projectile proj = Main.projectile[who];
			proj.velocity = proj.GetArcVel(Target.Center, GroveBoulder.Gravity, SecondPhase ? 12 : 9);
			(proj.ModProjectile as GroveBoulder).Controlled = false;

			_controlledWhoAmI.Remove(who);

			if (_controlledWhoAmI.Count == 0)
			{
				State = AIState.Idle;
				Timer = 0;
			}
			else
			{
				Timer -= 20;
			}
		}
	}

	private void RainProjectileBehaviour()
	{
		Timer++;

		if (Main.netMode != NetmodeID.Server)
		{
			Vector2 rotation = (Main.rand.NextFloat() * (MathHelper.Pi * 2f)).ToRotationVector2();
			PunchCameraModifier modifier = new(NPC.Center, rotation, 0.8f, 3, 120, 4000, "Grovetender");
			Main.instance.CameraModifiers.Add(modifier);
		}

		int speed = SecondPhase ? 6 : 7;

		if (Timer > 20 && Timer < 120 && Timer % 7 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			const int Variance = 80;

			while (true)
			{
				int x = (int)(NPC.Center.X / 16f) + Main.rand.Next(-Variance, Variance);
				int y = (int)(NPC.Center.Y / 16f) - 30;

				while (!Main.tile[x, y].HasTile || !Main.tile[x, y].HasUnactuatedTile || !ValidRainTiles.Contains(Main.tile[x, y].TileType))
				{
					y--;

					if (y < 20)
					{
						break;
					}
				}

				if (y < 20)
				{
					continue;
				}

				y++;

				var vel = new Vector2(0, Main.rand.NextFloat(1, 5));
				Vector2 position = new Vector2(x, y).ToWorldCoordinates();
				Tile above = Main.tile[x, y - 1];
				int count = 12;
				bool canEntDust = false;
				int damage = ModeUtils.ProjectileDamage(50);

				if (above.TileType == TileID.LeafBlock && (Timer == speed * 7 || Timer == 15 * speed))
				{
					count = 20;
					canEntDust = true;

					int type = ModContent.ProjectileType<FallingEntling>();
					Projectile.NewProjectile(NPC.GetSource_FromAI(), position, vel, type, damage, 1, Main.myPlayer, 120, Main.rand.Next(3));
				}
				else
				{
					int type = above.TileType == TileID.LivingWood ? ModContent.ProjectileType<FallingStick>() : ModContent.ProjectileType<FallingBranch>();
					Projectile.NewProjectile(NPC.GetSource_FromAI(), position, vel, type, damage, 1, Main.myPlayer, 120);
				}

				for (int i = 0; i < count; ++i)
				{
					var velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(4, 10));
					int type = canEntDust && i < count / 3 ? ModContent.DustType<EntDust>() : (above.TileType == TileID.LivingWood ? DustID.WoodFurniture : DustID.Grass);
					Dust.NewDustPerfect(position, type, velocity, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
				}

				break;
			}
		}
		else if (Timer > 160)
		{
			Timer = 0;
			State = AIState.Idle;
		}
	}

	private void RootDigBehaviour()
	{
		Timer++;

		if (Timer == 1)
		{
			DigRoot(NPC.Center.X / 16 - 18);
			DigRoot(NPC.Center.X / 16 - 71);
			DigRoot(NPC.Center.X / 16 + 27);
			DigRoot(NPC.Center.X / 16 + 59);
		}

		if (Timer == 60)
		{
			State = AIState.Idle;
		}
	}

	private void DigRoot(float xPosition)
	{
		int x = (int)xPosition;
		int y = (int)(NPC.Center.Y / 16f);

		while (WorldGen.SolidOrSlopedTile(x, y))
		{
			y--;
		}

		y++;

		Point16 smashPos = new(x, y);
		_rootPositionsAndTimers.Add(smashPos, 0);

		FastNoiseLite noise = new();
		noise.SetFrequency(0.07f);

		Vector2 position = new Vector2(x, Main.maxTilesY - 30).ToWorldCoordinates();
		int damage = ModeUtils.ProjectileDamage(120, 180, 300);
		Projectile.NewProjectile(NPC.GetSource_FromAI(), position, Vector2.Zero, ModContent.ProjectileType<LargeRoots>(), damage, 0, Main.myPlayer, (y - 5) * 16);

		if (Main.netMode != NetmodeID.Server)
		{
			Vector2 rotation = (Main.rand.NextFloat() * (MathHelper.Pi * 2f)).ToRotationVector2();
			PunchCameraModifier modifier = new(smashPos.ToWorldCoordinates(), rotation, 8f, 3, 100, 3000, "Grovetender");
			Main.instance.CameraModifiers.Add(modifier);
		}
	}

	private void UpdatePoweredRunestones()
	{
		foreach (Point16 position in PoweredRunestonePositions.Keys)
		{
			Vector2 pos = position.ToWorldCoordinates();
			bool increase = false;

			foreach (Player plr in Main.ActivePlayers)
			{
				if (!plr.dead && plr.DistanceSQ(pos) < 300 * 300)
				{
					increase = true;
				}
			}

			if (increase)
			{
				PoweredRunestonePositions[position] += SecondPhase ? 1.5f : 1;

				if (PoweredRunestonePositions[position] > MaxRunestoneWait)
				{
					PoweredRunestonePositions[position] = 0;

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<RunestoneBurst>(), 40, 0, Main.myPlayer);
					}

					SoundEngine.PlaySound(SoundID.Item100 with { PitchRange = (-0.2f, 0.2f) }, pos);
				}
			}
			else
			{
				PoweredRunestonePositions[position]--;

				if (PoweredRunestonePositions[position] < 0)
				{
					PoweredRunestonePositions[position] = 0;
				}
			}
		}
	}

	private void UpdateRootOpenings()
	{
		foreach (Point16 pos in _rootPositionsAndTimers.Keys)
		{
			bool anyoneNearby = false;

			foreach (Player player in Main.ActivePlayers)
			{
				if (!player.dead && Math.Abs(player.Center.X - pos.X * 16) < 200)
				{
					anyoneNearby = true;
					break;
				}
			}

			if (anyoneNearby)
			{
				_rootPositionsAndTimers[pos] += 4f;
			}

			if (_rootPositionsAndTimers[pos] > 600 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = 0; i < 3; ++i)
				{
					Vector2 position = new((pos.X + i * 1.2f) * 16, (Main.maxTilesY - 40) * 16);
					int damage = ModeUtils.ProjectileDamage(60);
					var velocity = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-10, -9));
					Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity, ModContent.ProjectileType<TinyBoulder>(), damage, 0, Main.myPlayer);
				}

				_rootPositionsAndTimers[pos] = 0;
			}
		}
	}

	private static void SpawnDust(Vector2 position, bool noLight)
	{
		Vector2 vel = new Vector2(0, -Main.rand.NextFloat(4, 7)).RotatedByRandom(0.1f);
		var dust = Dust.NewDustPerfect(position, ModContent.DustType<EntDust>(), vel, 0, default, Main.rand.NextFloat(1, 1.3f));
		dust.noLight = noLight;
	}
}