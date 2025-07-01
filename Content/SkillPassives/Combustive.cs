using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Skills.Magic;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives;

internal class Combustive(SkillTree tree) : SkillPassive(tree)
{
	internal class CombustionBlast : SkillProjectile<Nova>
	{
		public const int AreaOfEffect = 140;

		public int Spread => Skill.GetTotalAreaOfEffect(AreaOfEffect * Projectile.scale);

		public override string Texture => "Terraria/Images/NPC_0";

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = new Vector2(16);
			Projectile.timeLeft = 2;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.AshTreeShake, new() { PositionInWorld = Projectile.Center });

			for (int i = 0; i < 15; ++i)
			{
				Vector2 position = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(Spread / 2);
				var dust = Dust.NewDustPerfect(position, DustID.Torch, position.DirectionFrom(Projectile.Center) * 1.5f, Scale: Main.rand.NextFloat(1, 2));
				dust.noGravity = true;
				dust.fadeIn = 1.1f;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float distanceSq = Projectile.Center.DistanceSQ(targetHitbox.Center());
			return distanceSq > MathF.Pow(Spread - 20, 2) && distanceSq < MathF.Pow(Spread + 20, 2);
		}
	}
}