using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class CenterOfTheUniverseMastery : Passive
{
	internal class CenterOfTheUniversePlayer : ModPlayer
	{
		internal float RotationTimer = 0;

		public override void ResetEffects()
		{
			RotationTimer++;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			CheckSpawnPlanet(Player.GetSource_OnHit(target), Player, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (proj.type != ModContent.ProjectileType<OrbitingPlanet>())
			{
				CheckSpawnPlanet(Player.GetSource_OnHit(target), Player, damageDone);
			}
		}

		private static void CheckSpawnPlanet(IEntitySource source, Player player, int damageDone)
		{
			if (player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<CenterOfTheUniverseMastery>(out float value))
			{
				int type = ModContent.ProjectileType<OrbitingPlanet>();

				if (GetNextPlanetIndex(player, out int planetIndex, out _) && value / 100f > Main.rand.NextFloat())
				{
					int damage = (int)(damageDone * 0.35f);

					Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, 6, player.whoAmI, 0, planetIndex, planetIndex switch
					{
						>= 6 => 1,
						_ => 0
					});
				}
			}
		}

		public static bool GetNextPlanetIndex(Player player, out int index, out int maxIndex)
		{
			int type = ModContent.ProjectileType<OrbitingPlanet>();
			int num = player.ownedProjectileCounts[type];
			index = -1;
			maxIndex = -1;

			if (num >= 10)
			{
				return false;
			}

			HashSet<int> foundIndices = [];

			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.owner == player.whoAmI && proj.ModProjectile is OrbitingPlanet planet)
				{
					maxIndex = Math.Max(maxIndex, planet.Index);
					foundIndices.Add(planet.Index);
				}
			}

			for (int i = 0; i < 10; ++i)
			{
				if (!foundIndices.Contains(i))
				{
					index = i;
					return true;
				}
			}

			return false;
		}
	}
}
