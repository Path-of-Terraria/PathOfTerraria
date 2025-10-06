using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Skills.Magic;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.NovaTree;

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

internal class ConcurrentBlasts(SkillTree tree) : SkillPassive(tree)
{
	/// <summary> A damage multiplier applied to targets based on <see cref="ConcurrentNPC.Vulnerable"/>. </summary>
	public const float BonusDamage = 1.15f;
	public override object[] TooltipArguments => [MathUtils.Percent(BonusDamage - 1)];
}

internal class ScorchingTouch(SkillTree tree) : SkillPassive(tree)
{
	internal class FieryPatch : SkillProjectile<Nova>
	{
		public override string Texture => "Terraria/Images/NPC_0";

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = new Vector2(20);
			Projectile.timeLeft = 180;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			if (Main.rand.NextBool(10))
			{
				Vector2 position = Projectile.position + new Vector2(Projectile.width * Main.rand.NextFloat(), 4);
				ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.WallOfFleshGoatMountFlames, new() { PositionInWorld = position });
			}
		}
	}
}

internal class ThunderClaps(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["5"];
}

internal class VolatileNova(SkillTree tree) : SkillPassive(tree);