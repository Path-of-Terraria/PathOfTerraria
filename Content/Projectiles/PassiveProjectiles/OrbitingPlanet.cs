using PathOfTerraria.Content.Passives.Magic.Masteries;
using ReLogic.Content;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class OrbitingPlanet : ModProjectile
{
	private static Asset<Texture2D> Rings = null;

	internal float Timer => Main.player[Projectile.owner].GetModPlayer<CenterOfTheUniverseMastery.CenterOfTheUniversePlayer>().RotationTimer;
	internal int Index => (int)Projectile.ai[1];
	internal int Ring => (int)Projectile.ai[2];

	public override void SetStaticDefaults()
	{
		Rings = ModContent.Request<Texture2D>(Texture + "Rings");

		Main.projFrames[Type] = 5;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(18);
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.frame = Main.rand.Next(5);
	}

	public override void AI()
	{
		Projectile.timeLeft++;

		Player owner = Main.player[Projectile.owner];
		Vector2 offset = GetOffset();
		Projectile.Center = owner.Center + offset - new Vector2(4, 0);
		Projectile.rotation = offset.ToRotation() + MathHelper.PiOver2;
	}

	private Vector2 GetOffset()
	{
		if (Ring == 0)
		{
			return new Vector2(100, 0).RotatedBy((Timer * 0.025f) + (Index * (MathHelper.TwoPi / 6f)));
		}
		else if (Ring == 1)
		{
			int index = Index - 5;
			return new Vector2(180, 0).RotatedBy(Timer * 0.035f + (Index * MathHelper.TwoPi / 4f));
		}
		else
		{
			int index = Index - 8;
			return new Vector2(260, 0).RotatedBy(Timer * 0.0475f + index / 2f * MathHelper.TwoPi);
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Player plr = Main.player[Projectile.owner];
		plr.statMana = Math.Min(plr.statMana + 5, plr.statManaMax2);
		plr.ManaEffect(5);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		// Draw only if I'm the first projectile to draw
		for (int i = 0; i < Projectile.whoAmI; ++i)
		{
			if (Main.projectile[i].active && Main.projectile[i].type == Type)
			{
				return true;
			}
		}

		bool drawFirst = CenterOfTheUniverseMastery.CenterOfTheUniversePlayer.GetNextPlanetIndex(Main.player[Projectile.owner], out int num, out int max) && max < 6;

		Player plr = Main.player[Projectile.owner];
		Vector2 pos = plr.Center + new Vector2(0, plr.gfxOffY) - Main.screenPosition;
		Rectangle? src = drawFirst ? new Rectangle(80, 82, 198, 198) : null;
		Main.spriteBatch.Draw(Rings.Value, pos.Floor(), src, Color.White, 0f, (src?.Size() ?? Rings.Size()) / 2f, 1f, SpriteEffects.None, 0);

		return true;
	}
}
