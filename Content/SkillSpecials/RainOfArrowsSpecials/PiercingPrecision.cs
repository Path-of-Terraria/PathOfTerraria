using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.SkillPassives.RainOfArrowsTree;
using PathOfTerraria.Content.Skills.Ranged;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.SkillSpecials.RainOfArrowsSpecials;

internal class PiercingPrecision(SkillTree tree) : SkillSpecial(tree)
{
	internal class PrecisionSpawnerProjectile : ModProjectile
	{
		public override string Texture => "Terraria/Images/NPC_0";

		private int ProjToShoot => (int)Projectile.ai[0];
		private ref float Counter => ref Projectile.ai[2];

		IEntitySource src = null;

		public override void SetDefaults()
		{
			Projectile.tileCollide = false;
			Projectile.timeLeft = 60;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			src = source;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Projectile.Center = Projectile.Center;

			int rate = 10 - RainOfArrows.GetSkillForProjectile(Projectile).Tree.GetStrength<Quickload>() * 2;

			if (Projectile.timeLeft % rate == 0)
			{
				if (Main.myPlayer == Projectile.owner)
				{
					Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-10, 10));
					Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.6f) * -10 * Main.rand.NextFloat(0.9f, 1.1f);
					int proj = Projectile.NewProjectile(src, pos, velocity, ProjToShoot, Projectile.damage, 2, player.whoAmI);

					Projectile projectile = Main.projectile[proj];
					projectile.GetGlobalProjectile<RainOfArrows.RainProjectile>().SetRainProjectile(Main.projectile[proj], Projectile.timeLeft == 8);
				}

				Counter++;
			}

			if (Counter > 5)
			{
				Projectile.Kill();
			}
		}
	}
}
