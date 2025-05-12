using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;

public sealed class LightningBurst : ModProjectile
{
	private Vector2 TargetGlass
	{
		get => new(Projectile.ai[1], Projectile.ai[2]);
		set => (Projectile.ai[1], Projectile.ai[2]) = (value.X, value.Y);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.tileCollide = false;
		Projectile.width = 34;
		Projectile.height = 34;
		Projectile.timeLeft = 2;
	}

	public override void OnKill(int timeLeft)
	{
		const int KillDistance = 3;

		if (!WorldGen.InWorld((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 20))
		{
			return;
		}

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<WormLightning>());
		}

		for (int i = 0; i < 15; i++)
		{
			var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Scale: Main.rand.NextFloat(2, 4));
			dust.velocity.Y = Main.rand.NextFloat(-1, 1);
			dust.velocity *= 4f;
			dust.noGravity = true;
		}

		Point16 center = Projectile.Center.ToTileCoordinates16();

		for (int i = center.X - KillDistance; i < center.X + KillDistance; ++i)
		{
			for (int j = center.Y - KillDistance; j < center.Y + KillDistance; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.Glass && tile.HasTile)
				{
					WorldGen.KillTile(i, j, false, false, true);

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						NetMessage.SendTileSquare(-1, i, j);
					}
				}
			}
		}
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(TargetGlass) * 10, 0.2f);
		Projectile.rotation += 0.1f;

		CheckResetGlassPosition();

		if (!WorldGen.InWorld((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 20))
		{
			Projectile.Kill();
			return;
		}

		Point tilePos = Projectile.Center.ToTileCoordinates();
		Tile tile = Main.tile[tilePos];

		if (tile.HasTile && tile.TileType == TileID.Glass)
		{
			Projectile.Kill();
			return;
		}

		UpdateDustEffects();
	}

	private void CheckResetGlassPosition()
	{
		Tile tile = Main.tile[TargetGlass.ToTileCoordinates16()];

		if (!tile.HasTile || tile.TileType != TileID.Glass)
		{
			TargetGlass = SunDevourerNPC.FindGlass(TargetGlass, 300, 120);

			Projectile.netUpdate = true;
		}
	}

	private void UpdateDustEffects()
	{
		if (!Main.rand.NextBool(5))
		{
			return;
		}

		var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
		dust.velocity *= 2f;
		dust.noGravity = true;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		for (int i = 0; i < 3; ++i)
		{
			lightColor = new Color(156, 246, 255, 0) * ((i + 1) / 3f);
			float scale = (i + 1.2f) / 3f + Main.rand.NextFloat(0.3f);
			float rotation = i % 2 == 0 ? -1 : 1;

			DrawProjectile(in lightColor, in scale, in rotation);
		}

		return false;
	}

	private void DrawProjectile(in Color lightColor, in float scale, in float rotation)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);
		Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
		Vector2 origin = frame.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);
		SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		float rot = Projectile.rotation * rotation;

		Main.EntitySpriteDraw(texture, position, frame, Projectile.GetAlpha(lightColor), rot, origin, Projectile.scale * scale, effects);
	}
}