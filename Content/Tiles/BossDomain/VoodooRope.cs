using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class VoodooRope : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		DustType = DustID.Rope;

		AddMapEntry(new Color(183, 163, 152));
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		tile.TileFrameX = 0;
		tile.TileFrameY = (short)(Main.tile[i, j - 1].TileType == Type ? 18 : 0);

		return false;
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		bool anyProj = false;

		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.type == ModContent.ProjectileType<VoodooRopeProj>())
			{
				anyProj = true;
				break;
			}
		}

		if (anyProj)
		{
			return;
		}

		int type = ModContent.ProjectileType<VoodooRopeProj>();
		Projectile.NewProjectile(new EntitySource_TileBreak(i, j), new Vector2(i, j).ToWorldCoordinates(), Vector2.Zero, type, 0, 0, 255);
	}

	public class VoodooRopeProj : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation += 0.02f;

			if (Projectile.lavaWet)
			{
				Projectile.Kill();
			}
		}

		public override void OnKill(int timeLeft)
		{
			NPC.SpawnWOF(Projectile.position);
		}
	}
}