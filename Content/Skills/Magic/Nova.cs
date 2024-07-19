using PathOfTerraria.Core.Mechanics;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Magic;

public class Nova : Skill
{
	public override int MaxLevel => 3;

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = (15 - Level) * 60;
		ManaCost = 20 + 5 * Level;
		Duration = 0;
	}

	public override void UseSkill(Player player)
	{
		if (!CanUseSkill(player))
		{
			return;
		}

		player.statMana -= ManaCost;
		Timer = Cooldown;

		int damage = (int)(player.HeldItem.damage * (2 + 0.5f * Level));
		Projectile.NewProjectile(new EntitySource_Misc("NovaSkill"), player.Center, Vector2.Zero, ModContent.ProjectileType<NovaProjectile>(), damage, 2);

		SoundEngine.PlaySound(player.HeldItem.UseSound, player.Center);
	}

	public override bool CanUseSkill(Player player)
	{
		return base.CanUseSkill(player) && player.HeldItem.CountsAsClass(DamageClass.Magic);
	}

	private class NovaProjectile : ModProjectile
	{
		private const int TotalRadius = 300;

		public override string Texture => "Terraria/Images/NPC_0";

		private int Spread => (int)((1 - Projectile.timeLeft / 30f) * TotalRadius);

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.timeLeft = 30;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			const float maxDustIterations = 61;

			for (int i = 0; i < maxDustIterations; ++i)
			{
				Vector2 spread = new Vector2(Spread, 0).RotatedBy(i / maxDustIterations * MathHelper.TwoPi);
				Vector2 position = Projectile.Center + spread;
				Dust dust = Dust.NewDustPerfect(position, DustID.Astra, spread / Spread * 6);
				dust.scale = 0.5f;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float distanceSq = Projectile.Center.DistanceSQ(targetHitbox.Center());
			return distanceSq > MathF.Pow(Spread - 20, 2) && distanceSq < MathF.Pow(Spread + 20, 2);
		}
	}
}