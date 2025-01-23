using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class FallingEntling : ModProjectile
{
	private ref float WaitTimer => ref Projectile.ai[0];
	private ref float Frame => ref Projectile.ai[1];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 20 * 60;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.Size = new(30);
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
	}

	public override bool CanHitPlayer(Player target)
	{
		return WaitTimer <= 0;
	}

	public override void AI()
	{
		Projectile.frame = (int)Frame;

		if (WaitTimer-- > 0)
		{
			return;
		}

		if (WaitTimer > -30)
		{
			Projectile.Opacity = -WaitTimer / 30;
		}

		Projectile.rotation += Projectile.velocity.Y * 0.005f;
		Projectile.velocity.Y += 0.1f;

		if (!Projectile.tileCollide && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
		{
			Projectile.tileCollide = true;
		}

		if (Main.rand.NextBool(20))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptionThorns, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}

	public override bool ShouldUpdatePosition()
	{
		return WaitTimer <= 0;
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 20; ++i)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptionThorns, Projectile.velocity.X, Projectile.velocity.Y);
		}

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			NPC.NewNPC(Projectile.GetSource_Death(), (int)Projectile.Center.X, (int)Projectile.Center.Y, Frame switch
			{
				0 => ModContent.NPCType<ClumsyEntling>(),
				1 => ModContent.NPCType<Entling>(),
				_ => ModContent.NPCType<EntlingAlt>(),
			});
		}

		SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return WaitTimer <= -2;
	}
}