using PathOfTerraria.Content.Tiles.Furniture;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

internal class TransmogrifyBookProjectile : GlobalProjectile
{
	public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
	{
		return entity.type == ProjectileID.ViciousPowder || entity.type == ProjectileID.VilePowder || entity.type == ProjectileID.PurificationPowder;
	}

	public override void AI(Projectile projectile)
	{
		int minX = (int)MathHelper.Max(projectile.position.X / 16f - 1, 0);
		int maxX = (int)MathHelper.Min(projectile.Right.X / 16f + 2, Main.maxTilesX);
		int minY = (int)MathHelper.Max(projectile.position.Y / 16f - 1, 0);
		int maxY = (int)MathHelper.Min(projectile.Bottom.Y / 16f + 2, Main.maxTilesY);
		Vector2 edgeCheck = default;

		for (int i = minX; i < maxX; i++)
		{
			for (int j = minY; j < maxY; j++)
			{
				edgeCheck.X = i * 16;
				edgeCheck.Y = j * 16;

				if (projectile.Left.X <= edgeCheck.X || projectile.position.X >= edgeCheck.X + 16f || projectile.Bottom.Y <= edgeCheck.Y || projectile.position.Y >= edgeCheck.Y + 16f || !Main.tile[i, j].HasTile)
				{
					continue;
				}

				if (projectile.type == ProjectileID.PurificationPowder)
				{
					Tile convert = Main.tile[i, j];

					if (convert.HasTile && convert.TileType == ModContent.TileType<EvilBook>())
					{
						convert.TileType = TileID.Books;
						convert.TileFrameX = (short)(Main.rand.Next(5) * 18);
						convert.TileFrameY = 0;

						WorldGen.SquareTileFrame(i, j);

						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.SendTileSquare(-1, i, j);
						}
					}
				}

				if (projectile.type == ProjectileID.VilePowder || projectile.type == ProjectileID.ViciousPowder)
				{
					Tile convert = Main.tile[i, j];

					if (convert.HasTile && convert.TileType == TileID.Books)
					{
						convert.TileType = (ushort)ModContent.TileType<EvilBook>();
						convert.TileFrameX = (short)(Main.rand.Next(3) * 18);
						convert.TileFrameY = 0;

						WorldGen.SquareTileFrame(i, j);

						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.SendTileSquare(-1, i, j);
						}
					}
				}
			}
		}
	}
}
