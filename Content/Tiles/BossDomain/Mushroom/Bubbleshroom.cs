using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mushroom;

internal class Bubbleshroom : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.BreakableWhenPlacing[Type] = false;
		TileID.Sets.PreventsSandfall[Type] = true;
		TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

		BubblerTE tileEntity = ModContent.GetInstance<BubblerTE>();
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, false);

		TileObjectData.newTile.Width = 4;
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Origin = new Point16(2, 1);
		TileObjectData.addTile(Type);

		DustType = DustID.GlowingMushroom;

		AddMapEntry(new Color(135, 137, 225));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];
		float str = Main.rand.Next(28, 42) * 0.005f;
		str += (270 - Main.mouseTextColor) / 1000f;

		if (tile.TileColor == PaintID.None)
		{
			r = 0f;
			g = 0.2f + str / 2f;
			b = 1f;
		}
		else
		{
			Color color2 = WorldGen.paintColor(tile.TileColor);
			r = color2.R / 255f;
			g = color2.G / 255f;
			b = color2.B / 255f;
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		Tile tile = Main.tile[i, j];
		i -= tile.TileFrameX / 18;
		j -= tile.TileFrameY / 18;
		ModContent.GetInstance<BubblerTE>().Kill(i, j);
	}

	private class BubbleshroomBubble : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.NoLiquidDistortion[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.Size = new Vector2(32);
			Projectile.extraUpdates = 0;
			Projectile.Opacity = 0f;
			Projectile.hide = false;
			Projectile.scale = Main.rand.NextFloat(0.8f, 1.2f);
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.light = 0;
		}

		public override void AI()
		{
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1, 0.05f);
			Projectile.velocity.Y = MathHelper.Max(Projectile.velocity.Y - 0.2f, -3f * Projectile.scale);

			if (Projectile.Opacity > 0.99f)
			{
				Projectile.tileCollide = true;
			}

			if (!Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
			{
				Projectile.velocity.Y += 0.3f;
				Projectile.timeLeft--;
			}
			else
			{
				Projectile.timeLeft = 60;
			}
			
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Hitbox.Intersects(Projectile.Hitbox))
				{
					player.breath = Math.Min(player.breath + 100, player.breathMax);
					Projectile.Kill();
					break;
				}
			}
		}

		public override void OnKill(int timeLeft)
		{
			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(SoundID.Item85, Projectile.Center);

				for (int i = 0; i < 18; ++i)
				{
					Vector2 vel = Main.rand.NextVector2CircularEdge(3, 3) * Main.rand.NextFloat(0.8f, 1.2f);
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(12, 12), DustID.BubbleBlock, vel, Scale: Main.rand.NextFloat(0.4f, 0.8f));
				}
			}
		}
	}

	public class BubblerTE : ModTileEntity
	{
		private int _timer = Main.rand.Next(3 * 60);

		public override bool IsTileValidForEntity(int x, int y)
		{
			return Main.tile[x, y].TileType == ModContent.TileType<Bubbleshroom>();
		}

		public override void Update()
		{
			_timer++;

			Vector2 worldPos = Position.ToWorldCoordinates();

			if (_timer > 4 * 60 && Collision.WetCollision(Position.ToWorldCoordinates(), 68, 26))
			{
				Vector2 position = Position.ToWorldCoordinates(Main.rand.NextFloat(54), Main.rand.NextFloat(-8, 12));
				int type = ModContent.ProjectileType<BubbleshroomBubble>();
				Projectile.NewProjectile(new EntitySource_TileUpdate(Position.X, Position.Y), position, Vector2.Zero, type, 0, 0, Main.myPlayer, 0, 0, 0);

				_timer = -Main.rand.Next(60);
			}
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
			int topLeftX = i - tileData.Origin.X;
			int topLeftY = j - tileData.Origin.Y;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
				return -1;
			}

			return Place(topLeftX, topLeftY);
		}
	}
}
