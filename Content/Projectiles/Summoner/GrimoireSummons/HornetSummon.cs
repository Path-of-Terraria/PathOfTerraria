using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Content.Items.Pickups.GrimoirePickups;
using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class HornetSummon : GrimoireSummon
{
	public override int BaseDamage => 16;

	private ref float Timer => ref Projectile.ai[1];
	private ref float HoneyTimer => ref Projectile.ai[2];

	public override void StaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.width = 26;
		Projectile.height = 34;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.rotation = Projectile.velocity.X * 0.1f;
		Projectile.spriteDirection = Projectile.direction = -Math.Sign(Projectile.velocity.X);

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.5f;
			
			if (Projectile.velocity.LengthSquared() > 8 * 8)
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 8;
			}

			Timer++;

			if (Timer > 20)
			{
				int npcIndex = -1;

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy() && (npcIndex == -1 || npc.DistanceSQ(Projectile.Center) < Main.npc[npcIndex].DistanceSQ(Projectile.Center)))
					{
						npcIndex = npc.whoAmI;
					}
				}

				if (npcIndex != -1 && Main.npc[npcIndex].DistanceSQ(Projectile.Center) < 800 * 800)
				{
					Vector2 velocity = Projectile.DirectionTo(Main.npc[npcIndex].Center) * 11;
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ProjectileID.HornetStinger, 16, 1f);
				}

				Timer = 0;
			}

			if (Projectile.honeyWet && !NPC.downedQueenBee)
			{
				HoneyTimer++;

				if (HoneyTimer > 3 * 60)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<QueenBeePortal>(), 0, 1f);

					Projectile.active = false;
				}
			}
		}
	}

	protected override void AltEffect()
	{
	}

	protected override void AnimateSelf()
	{
		Projectile.frame = (int)((AnimationTimer += 0.3f * (Projectile.velocity.Length() / 16f + 0.5f)) % 3);
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
			{ ModContent.ItemType<BatWings>(), 1 }, { ModContent.ItemType<LargeStinger>(), 1 }
		};
	}
}
