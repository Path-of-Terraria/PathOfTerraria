using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal partial class Grovetender : ModNPC
{
	public const int MaxRunestoneWait = 60 * 2;

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
					poweredRunestonePositions.Add(new Point16(i, j), 0);
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
			ControlledWhoAmI = Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, type, ModeUtils.ProjectileDamage(80), 8, Main.myPlayer);
		}
		else if (Timer > 1 && Timer < 100)
		{
			Projectile proj = Main.projectile[ControlledWhoAmI];
			Vector2 targetPos = NPC.Center - new Vector2(0, 300);

			if (Timer > 80 && Timer < 90)
			{
				targetPos += proj.DirectionFrom(Target.Center) * 340 * ((Timer - 80) / 10f);
			}
			else if (Timer >= 90)
			{
				targetPos += proj.DirectionFrom(Target.Center) * 340 * (1 - (Timer - 90) / 10f);
			}

			proj.Center = Vector2.Lerp(proj.Center, targetPos, 0.06f);
			proj.velocity = Vector2.Zero;
			(proj.ModProjectile as GroveBoulder).Controlled = true;
		}
		else if (Timer == 100)
		{
			Projectile proj = Main.projectile[ControlledWhoAmI];
			proj.velocity = proj.GetArcVel(Target.Center, GroveBoulder.Gravity, 9);
			(proj.ModProjectile as GroveBoulder).Controlled = false;

			State = AIState.Idle;
			Timer = 0;
		}
	}

	private void RainProjectileBehaviour()
	{
		Timer++;

		if (Timer > 20 && Timer < 120 && Timer % 7 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			while (true)
			{
				int x = (int)NPC.Center.X / 16 + Main.rand.Next(-80, 80);
				int y = (int)NPC.Center.Y / 16;

				while (!Main.tile[x, y].HasTile || !Main.tile[x, y].HasUnactuatedTile || Main.tile[x, y].TileType is not TileID.LivingWood and not TileID.LeafBlock)
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

				if (above.TileType == TileID.LeafBlock && Timer is 49 or 105)
				{
					count = 20;
					canEntDust = true;

					int type = ModContent.ProjectileType<FallingEntling>();
					Projectile.NewProjectile(NPC.GetSource_FromAI(), position, vel, type, ModeUtils.ProjectileDamage(30), 1, Main.myPlayer, 120, Main.rand.Next(3));
				}
				else
				{
					int type = above.TileType == TileID.LivingWood ? ModContent.ProjectileType<FallingStick>() : ModContent.ProjectileType<FallingBranch>();
					Projectile.NewProjectile(NPC.GetSource_FromAI(), position, vel, type, ModeUtils.ProjectileDamage(30), 1, Main.myPlayer, 120);
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

	private void UpdatePoweredRunestones()
	{
		foreach (Point16 position in poweredRunestonePositions.Keys)
		{
			Vector2 pos = position.ToWorldCoordinates();
			bool increase = false;

			foreach (Player plr in Main.ActivePlayers)
			{
				if (plr.DistanceSQ(pos) < 300 * 300)
				{
					increase = true;
				}
			}

			if (increase)
			{
				poweredRunestonePositions[position]++;

				if (poweredRunestonePositions[position] > 60 * 8)
				{
					poweredRunestonePositions[position] = 0;

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<RunestoneBurst>(), 0, 0, Main.myPlayer);
					}
				}
			}
			else
			{
				poweredRunestonePositions[position]--;

				if (poweredRunestonePositions[position] < 0)
				{
					poweredRunestonePositions[position] = 0;
				}
			}
		}
	}
}