using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Summon;

public class PestSwarm : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (5 - Level) * 60;
		ManaCost = 10 - Level * 3;
		Duration = SentryNPC.DefaultSentryDuration;
		WeaponType = ItemType.None;
	}

	public override bool CanUseSkill(Player player, ref SkillFailure failReason, bool justChecking = true)
	{
		if (Collision.SolidCollision(GetTarget(player) - new Vector2(12, 12), 24, 24))
		{
			failReason = new SkillFailure(SkillFailReason.Other, "Blocked");
			return false;
		}

		return base.CanUseSkill(player, ref failReason, justChecking);
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);

		Vector2 pos = GetTarget(player);
		int type = ModContent.ProjectileType<LocustSpawnCircle>();
		Projectile.NewProjectile(new EntitySource_UseSkill(player, this), pos, Vector2.Zero, type, 20 * Level, 0, player.whoAmI, TotalDuration);
	}

	private static Vector2 GetTarget(Player player)
	{
		return player.Center + player.DirectionTo(Main.MouseWorld) * MathF.Min(player.Distance(Main.MouseWorld), 120);
	}

	public class LocustSpawnCircle : SkillProjectile<PestSwarm>
	{
		const float MaxTime = 24;

		private ref float Duration => ref Projectile.ai[0];
		private ref float TimeLeft => ref Projectile.ai[1];

		public override void SetDefaults()
		{
			Projectile.timeLeft = 120;
			Projectile.Opacity = 0;
			Projectile.Size = new Vector2(20);
			Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindProjectiles.Add(index);
		}

		public override bool? CanDamage()
		{
			return false;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 120)
			{
				Projectile.scale = 0f;
				TimeLeft = MaxTime;
			}

			Projectile.scale += 1 / (MaxTime + 5);

			if (Projectile.timeLeft > MaxTime / 2)
			{
				Projectile.Opacity += 1 / (MaxTime / 2f);
			}
			else
			{
				Projectile.Opacity -= 1 / (MaxTime / 2f);
			}

			if (TimeLeft-- <= 0)
			{
				Projectile.Kill();
			}

			if (TimeLeft == (int)MaxTime / 2 && Main.myPlayer == Projectile.owner)
			{
				int type = ModContent.ProjectileType<SimpleLocust>();
				var src = new EntitySource_UseSkill(Main.player[Projectile.owner], Skill);
				Projectile.NewProjectile(src, Projectile.Center, new Vector2(0, -1), type, Projectile.damage, 0, Projectile.owner, 0, 0, Duration);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Color color = lightColor * Projectile.Opacity * 0.75f;
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color, 0f, tex.Size() / 2f, Projectile.scale * 1.25f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color * 0.33f, 0f, tex.Size() / 2f, Projectile.scale * 1.65f, SpriteEffects.None, 0);
			return true;
		}
	}

	public class SimpleLocust : ModProjectile
	{
		private bool Initialized
		{
			get => Projectile.ai[0] == 1;
			set => Projectile.ai[0] = value ? 1 : 0;
		}

		private int Target
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private ref float TimeLeft => ref Projectile.ai[2];

		private ref float Timer => ref Projectile.localAI[0];

		public override void SetDefaults()
		{
			Projectile.Size = new(26, 18);
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.Opacity = 0f;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void AI()
		{
			if (!Initialized)
			{
				Initialized = true;
				Target = -1;

				FindTarget();
			}
			else if (Target == -1 || InvalidTarget())
			{
				FindTarget();
			}

			if (Target != -1)
			{
				Chase();
			}
			else
			{
				Idle();
			}

			if (TimeLeft-- <= 0)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			if (Math.Abs(Projectile.velocity.X) > 0.1f)
			{
				Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);
			}
		}

		private bool InvalidTarget()
		{
			NPC npc = Main.npc[Target];
			return !npc.CanBeChasedBy() || npc.DistanceSQ(Projectile.Center) > 600 * 600;
		}

		private void Idle()
		{
			if (Projectile.velocity.Y == 0)
			{
				Timer++;
				Projectile.velocity.X = 0;

				if (Timer > 45)
				{
					Projectile.velocity.X = Main.rand.NextFloat(-1, 1);
					Projectile.velocity.Y = -2;
					Projectile.netUpdate = true;
					
					Timer = 0;
				}
			}
		}

		private void Chase()
		{
			NPC target = Main.npc[Target];
			float targetSpeedX = MathF.Sign(target.Center.X - Projectile.Center.X) * 2.5f;

			if (Projectile.velocity.Y == 0)
			{
				Timer++;
				Projectile.velocity.X = 0;

				if (Timer > 15)
				{
					Projectile.velocity.X = targetSpeedX;
					Projectile.velocity.Y = -4;
					Timer = 0;
				}
			}
			else
			{
				Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, targetSpeedX, 0.01f);
			}
		}

		private void FindTarget()
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy())
				{
					continue;
				}

				float dist = npc.DistanceSQ(Projectile.Center);

				if (dist < 400 * 400 && (Target == -1 || Main.npc[Target].DistanceSQ(Projectile.Center) > dist))
				{
					Target = npc.whoAmI;
					Timer = 0;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 17; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, i < 8 ? DustID.Blood : ModContent.DustType<LocustDust>());
			}
		}
	}
}

public class LocustDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.alpha = 0;
	}

	public override Color? GetAlpha(Dust dust, Color lightColor)
	{
		return lightColor;
	}
}