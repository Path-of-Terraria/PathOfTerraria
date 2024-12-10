using System.Collections.Generic;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class DemonSummon : GrimoireSummon
{
	public override int BaseDamage => 52;

	private ref float Timer => ref Projectile.ai[1];

	public override void StaticDefaults()
	{
		Main.projFrames[Type] = 5;
	}

	public override void SetDefaults()
	{
		Projectile.width = 52;
		Projectile.height = 52;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.extraUpdates = 1;
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		modifiers.FinalDamage -= 0.6f;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.rotation = Projectile.velocity.X * 0.1f;
		Projectile.spriteDirection = Projectile.direction = -Math.Sign(Projectile.velocity.X);

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.15f;

			if (Projectile.velocity.LengthSquared() > 6 * 6)
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 6;
			}

			Timer++;

			if (Timer > 30)
			{
				int npcIndex = -1;

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy() && (npcIndex == -1 || npc.lifeMax > Main.npc[npcIndex].lifeMax))
					{
						npcIndex = npc.whoAmI;
					}
				}

				if (npcIndex != -1 && Main.npc[npcIndex].DistanceSQ(Projectile.Center) < 800 * 800)
				{
					Vector2 velocity = Projectile.DirectionTo(Main.npc[npcIndex].Center) * 14 + Main.npc[npcIndex].velocity * 2;
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ModContent.ProjectileType<DemonWeapons>(), Projectile.damage, 1f);
				}

				Timer = 0;
			}
		}
	}

	protected override void AltEffect()
	{
	}

	protected override void AnimateSelf()
	{
		Projectile.frame = (int)((AnimationTimer += 0.1f * (Projectile.velocity.Length() / 16f + 0.5f)) % 5);
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
		{
			Projectile.velocity.X = -oldVelocity.X * 0.9f;
		}

		if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
		{
			Projectile.velocity.Y = -oldVelocity.Y * 0.9f;
		}

		return false;
	}

	public override Dictionary<int, int> GetRequiredParts()
	{
		return new Dictionary<int, int>()
		{
			{ ModContent.ItemType<SoulfulAsh>(), 3 }, { ModContent.ItemType<FlamingEye>(), 1 }
		};
	}
}
