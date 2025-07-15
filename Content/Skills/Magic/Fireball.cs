using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Magic;

public class Fireball : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 8 * 60;
		ManaCost = 20 + 10 * Level;
		Duration = 0;
		WeaponType = ItemType.None;
	}

	public override void UseSkill(Player player)
	{
		base.UseSkill(player);

		int damage = GetTotalDamage((Level - 1) * 20 + 30);
		var source = new EntitySource_UseSkill(player, this);
		float knockback = 2f;
		int type = ModContent.ProjectileType<FireballProj>();
		Vector2 velocity = player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.05f) * 8 * Main.rand.NextFloat(0.9f, 1.1f);
		
		Projectile.NewProjectile(source, player.Center - new Vector2(0, 12), velocity, type, damage, knockback, player.whoAmI, Level);
		SoundEngine.PlaySound(SoundID.Item20 with { PitchRange = (-0.8f, 0.2f) }, player.Center);
	}

	private class FireballProj : SkillProjectile<Fireball>
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetType().Name;

		private ref float Level => ref Projectile.ai[0];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 5;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.timeLeft = 240;
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.scale = 0f;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.frame = (int)(Projectile.frameCounter++ / 5f) % 5;
			Projectile.Opacity = (Projectile.velocity.Length() - 2) / 8f * 0.25f + 0.75f;
			Projectile.scale = Math.Min(1, Projectile.scale + 0.025f);
			Projectile.width = (int)(36 * Projectile.scale);
			Projectile.height = (int)(36 * Projectile.scale); 

			if (Projectile.scale == 1)
			{
				Projectile.velocity.Y += 0.01f;
			}

			SpawnDust(1, 0.4f);
		}

		private void SpawnDust(int count, float strength = 1f)
		{
			for (int i = 0; i < count; ++i)
			{
				float scale = Main.rand.NextFloat(0.8f, 1.4f);
				Vector2 vel = Projectile.velocity * strength;
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, vel.X, vel.Y, Scale: scale);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextFloat() < 0.05f + Level * 0.1f)
			{
				target.AddBuff(BuffID.OnFire, 3 * 60);
				SpawnDust(12);
			}
		}

		public override void OnKill(int timeLeft)
		{
			if (!Main.dedServ)
			{
				SpawnDust(20);
				SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Variants = [1, 2], PitchRange = (0.2f, 0.6f) }, Projectile.Center);

				for (int i = 0; i < 30; ++i)
				{
					float scale = Main.rand.NextFloat(1.2f, 2f);
					Vector2 vel = Projectile.velocity + Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.3f, 1f);
					Vector2 pos = Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height));
					Dust.NewDustPerfect(pos, DustID.Torch, vel, Scale: scale);
				}

				for (int i = 0; i < 8; ++i)
				{
					Vector2 vel = Projectile.velocity + Main.rand.NextVector2CircularEdge(4, 4) * Main.rand.NextFloat(0.3f, 1f);
					Gore.NewGorePerfect(Projectile.GetSource_Death(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
				}
			}

			int area = Skill.GetTotalAreaOfEffect(160);
			Projectile.Resize(area, area);
			Projectile.Damage();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			int frameHeight = tex.Height / Main.projFrames[Type];
			var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Color col = lightColor * Projectile.Opacity;
			Vector2 position = Projectile.Center - Main.screenPosition;
			Vector2 origin = src.Size() / new Vector2(1.5f, 2);
			
			Main.EntitySpriteDraw(tex, position, src, col * Projectile.scale, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}