using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal partial class Grovetender : ModNPC
{
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
		else if (Timer > 1 && Timer < 120)
		{
			Projectile proj = Main.projectile[ControlledWhoAmI];
			Vector2 targetPos = NPC.Center - new Vector2(0, 300);

			if (Timer > 100 && Timer < 110)
			{
				targetPos += proj.DirectionFrom(Target.Center) * 340 * ((Timer - 100) / 10f);
			}
			else if (Timer >= 110)
			{
				targetPos += proj.DirectionFrom(Target.Center) * 340 * (1 - (Timer - 110) / 10f);
			}

			proj.Center = Vector2.Lerp(proj.Center, targetPos, 0.04f);
			proj.velocity = Vector2.Zero;
			(proj.ModProjectile as GroveBoulder).Controlled = true;
		}
		else if (Timer == 120)
		{
			Projectile proj = Main.projectile[ControlledWhoAmI];
			proj.velocity = proj.GetArcVel(Target.Center, GroveBoulder.Gravity, 9);

			State = AIState.Idle;
			Timer = 0;
		}
	}

	private void RainProjectileBehaviour()
	{
		Timer++;

		if (Timer > 20 && Timer < 120 && Timer % 3 == 0)
		{
			int x = (int)NPC.Center.X / 16;
			int y = (int)NPC.Center.Y / 16;

			while (!Main.tile[x, y].HasTile || !Main.tile[x, y].HasUnactuatedTile || Main.tile[x, y].TileType is not TileID.LivingWood and not TileID.LeafBlock)
			{
				y--;
			}

			y++;

			var vel = new Vector2(0, Main.rand.NextFloat(1, 5));
			Vector2 position = new Vector2(x, y).ToWorldCoordinates();
			Projectile.NewProjectile(NPC.GetSource_FromAI(), position, vel, ModContent.ProjectileType<FallingStick>(), ModeUtils.ProjectileDamage(30), 1);
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