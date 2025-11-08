using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class AutomagicMissileMastery : Passive
{
	internal class AutomagicMissilePlayer : ModPlayer
	{
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<AutomagicMissileMastery>(out float value) || Main.rand.NextFloat() > value / 100f)
			{
				return;
			}

			if (proj.type != ProjectileID.MagicMissile || !proj.TryGetGlobalProjectile(out AutomagicMissileProjectile auto) || !auto.Automagic)
			{
				int nextTarget = GetTarget(Player);

				if (nextTarget != -1)
				{
					Vector2 velocity = new Vector2(0, -6).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.9f, 1.1f);
					int damage = Math.Max(1, damageDone / 2);
					int p = Projectile.NewProjectile(proj.GetSource_OnHit(target), Player.Center, velocity, ProjectileID.MagicMissile, damage, 0, Player.whoAmI);
					Projectile projectile = Main.projectile[p];

					if (projectile.TryGetGlobalProjectile(out AutomagicMissileProjectile newAuto))
					{
						newAuto.Target = (short)nextTarget;
						newAuto.Automagic = true;

						projectile.extraUpdates = 1;
					}
				}
			}
		}

		private static int GetTarget(Player player)
		{
			PriorityQueue<int, float> options = new();

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.CanBeChasedBy() && player.Center.DistanceSQ(npc.Center) < 800 * 800 && Collision.CanHit(player, npc))
				{
					options.Enqueue(npc.whoAmI, Main.rand.NextFloat());
				}
			}

			return options.Count == 0 ? -1 : options.Dequeue();
		}
	}

	internal class AutomagicMissileProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		internal bool Automagic = false;
		internal short Target = -1;

		private byte _cap = 60;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.type == ProjectileID.MagicMissile;
		}

		public override bool PreAI(Projectile projectile)
		{
			if (Automagic)
			{
				if (_cap > 0)
				{
					_cap--;
					return false;
				}

				if (Target == -1)
				{
					projectile.Kill();
					return false;
				}

				NPC npc = Main.npc[Target];

				if (!npc.CanBeChasedBy())
				{
					projectile.Kill();
					return false;
				}

				projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.DirectionTo(npc.Center) * 12, 0.05f);
				projectile.rotation = projectile.velocity.ToRotation();
				return false;
			}

			return true;
		}
	}
}
