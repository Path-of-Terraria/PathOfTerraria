using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class AntlerShardProj : ModProjectile
{
	public Vector2 Target => new(Projectile.ai[0], Projectile.ai[1]);

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 6;
	}

	public override void SetDefaults()
	{
		Projectile.timeLeft = 2;
		Projectile.Size = new Vector2(16);
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.frame = Main.rand.Next(6);
	}

	public override bool CanHitPlayer(Player target)
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		float dist = Projectile.Distance(Target);
		Projectile.velocity = Projectile.DirectionTo(Target) * (2 + dist / 50f);

		if (dist < 6f)
		{
			Projectile.Kill();
		}
	}

	public override void OnKill(int timeLeft)
	{
		Point16 pos = Projectile.Center.ToTileCoordinates16();
		WorldGen.PlaceTile(pos.X, pos.Y, ModContent.TileType<AntlerPieces>(), true, false, -1, Main.rand.Next(6));

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendTileSquare(-1, pos.X, pos.Y);
		}
	}
}
