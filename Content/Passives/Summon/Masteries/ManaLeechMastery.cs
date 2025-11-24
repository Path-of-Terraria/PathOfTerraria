using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Summoner;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class ManaLeechMastery : Passive
{
	internal class ManaLeechProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private int _manaTimer = 0;
		private float _manaCost = 0;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.ModProjectile is GrimoireSummon;
		}

		public override bool PreAI(Projectile projectile)
		{
			if (!projectile.TryGetOwner(out Player plr) || !plr.GetModPlayer<PassiveTreePlayer>().HasNode<ManaLeechMastery>())
			{
				return true;
			}

			_manaTimer++;
			_manaCost += _manaTimer * 0.001f;

			if ((int)_manaCost >= 1)
			{
				if (plr.CheckMana((int)_manaCost, true))
				{
					_manaCost -= (int)_manaCost;
					plr.manaRegenDelay = 20;

					Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.ManaRegeneration, projectile.velocity.X, projectile.velocity.Y, 150);
				}
				else
				{
					plr.channel = false;
				}
			}

			return true;
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.FinalDamage += _manaTimer * 0.002f;
		}
	}
}