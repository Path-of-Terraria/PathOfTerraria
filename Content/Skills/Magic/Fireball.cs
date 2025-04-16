using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
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
		player.statMana -= ManaCost;
		Timer = Cooldown;

		int damage = (Level - 1) * 20 + 30;
		var source = new EntitySource_UseSkill(player, this);
		float knockback = 2f;
		int type = ModContent.ProjectileType<FireballProj>();

		Projectile.NewProjectile(source, player.Center, player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.05f) * 8, type, damage, knockback, player.whoAmI, Level);
		SoundEngine.PlaySound(SoundID.Item20 with { PitchRange = (-0.8f, 0.2f) }, player.Center);
	}

	private class FireballProj : ModProjectile
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetType().Name;

		private ref float Level => ref Projectile.ai[0];

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.timeLeft = 160;
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.frame = Projectile.timeLeft % 15 / 5;
			Projectile.velocity.Y += 0.02f;
			Projectile.velocity *= 0.996f;
			Projectile.Opacity = (Projectile.velocity.Length() - 2) / 8f * 0.25f + 0.75f;

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

				SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Variants = [1, 2], Volume = 0.6f }, Projectile.Center);
			}
		}

		public override void OnKill(int timeLeft)
		{
			SpawnDust(20);
			SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Variants = [1, 2], PitchRange = (0.2f, 0.6f) }, Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			int frameHeight = tex.Height / Main.projFrames[Type];
			var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Color col = lightColor * Projectile.Opacity;

			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, src, col, Projectile.rotation, new(55, 11), 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}