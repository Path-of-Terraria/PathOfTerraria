using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Magic;

public class IceBolt : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (int)((5.5f - 0.5f * Level) * 60);
		ManaCost = 6 + 6 * Level;
		Duration = 0;
		WeaponType = ItemType.None;
	}

	public override void UseSkill(Player player)
	{
		player.statMana -= ManaCost;
		Timer = Cooldown;

		int damage = Level * 10 + 5;
		var source = new EntitySource_UseSkill(player, this);
		float knockback = 2f;
		int type = ModContent.ProjectileType<IceBoltProj>();

		Projectile.NewProjectile(source, player.Center, player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.05f) * 8, type, damage, knockback, player.whoAmI, Level);
		SoundEngine.PlaySound(SoundID.Item20 with { PitchRange = (-0.8f, 0.2f) }, player.Center);
	}

	private class IceBoltProj : ModProjectile
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetType().Name;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 160;
			Projectile.penetrate = 1;
			Projectile.aiStyle = -1;
			Projectile.extraUpdates = 1;
			Projectile.frame = Main.rand.Next(3);
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Main.rand.NextBool(8))
			{
				SpawnDust(1, 0.7f);
			}
		}

		private void SpawnDust(int count, float strength = 1f)
		{
			for (int i = 0; i < count; ++i)
			{
				float scale = Main.rand.NextFloat(0.8f, 1.4f);
				Vector2 vel = Projectile.velocity * strength;
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, vel.X, vel.Y, Scale: scale);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Chilled, 8 * 60);
		}

		public override void OnKill(int timeLeft)
		{
			SpawnDust(20, 2f);
			SoundEngine.PlaySound(SoundID.Shatter with { PitchRange = (0.2f, 0.6f), Volume = 0.4f }, Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			int frameHeight = tex.Height / Main.projFrames[Type];
			var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Color col = lightColor * Projectile.Opacity;

			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, src, col, Projectile.rotation, new(26, 8), 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}