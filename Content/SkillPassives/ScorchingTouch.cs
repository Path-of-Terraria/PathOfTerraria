using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Skills.Magic;
using Terraria.GameContent.Drawing;

namespace PathOfTerraria.Content.SkillPassives;

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