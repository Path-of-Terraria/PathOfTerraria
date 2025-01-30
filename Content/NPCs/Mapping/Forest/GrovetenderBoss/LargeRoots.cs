using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class LargeRoots : ModProjectile
{
	private ref float TargetY => ref Projectile.ai[0];

	private bool Return
	{
		get => Projectile.ai[1] == 1;
		set => Projectile.ai[1] = value ? 1 : 0;
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 20 * 60;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.Size = new(36);
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.extraUpdates = 1;
	}

	public override void AI()
	{
		if (Return)
		{
			Projectile.velocity.Y += 0.03f;

			if (Projectile.Center.Y > (Main.maxTilesY - 40) * 16)
			{
				Projectile.Kill();
			}
		}
		else
		{
			Projectile.velocity.Y = -8;

			if (Projectile.Center.Y < TargetY)
			{
				Projectile.velocity.Y = 0;
				Return = true;
			}

			Dust.NewDust(new Vector2(Projectile.Center.X, TargetY), 2, 2, DustID.Dirt, 0, -12);

			int x = (int)(Projectile.Center.X / 16);
			int j = (int)(Projectile.Center.Y / 16f);

			for (int i = x - 2; i < x + 3; ++i)
			{
				bool hadTile = Main.tile[i, j].HasTile;

				WorldGen.KillTile(i, j, false, false, true);

				if (hadTile)
				{
					Tile tile = Main.tile[i, j];
					tile.WallType = WallID.None;

					WorldGen.PlaceWall(i, j, WallID.LivingWood, true);
				}
			}
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 12; ++i)
		{
			int type = !Main.rand.NextBool(3) ? DustID.Grass : DustID.WoodFurniture;
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type, Projectile.velocity.X, Projectile.velocity.Y);
		}

		SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 pos = Projectile.Center - Main.screenPosition;
		float dist = (Projectile.Center.X - (Main.maxTilesY - 40)) / 22f;
		Rectangle rootSource = new(0, 56, 34, 22);

		for (int i = 1; i < dist; ++i)
		{
			Main.EntitySpriteDraw(tex, pos + new Vector2(0, i * 22), rootSource, lightColor, 0f, rootSource.Size() / 2f, 1f, SpriteEffects.None);
		}

		Rectangle pointSource = new(0, 0, 34, 54);
		Main.EntitySpriteDraw(tex, pos, pointSource, lightColor, 0f, pointSource.Size() / 2f, 1f, SpriteEffects.None);
		return false;
	}
}