using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class LimitedEntranceMastery : Passive
{
	internal class LimitedEntranceProjectile : GlobalProjectile 
	{
		public override bool InstancePerEntity => true;

		private int _timeAlive = 0;
		private int _buffTime = -1;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (projectile.owner is < 0 or >= Main.maxPlayers)
			{
				return;
			}

			Player plr = Main.player[projectile.owner];
			if (plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<LimitedEntranceMastery>(out float value))
			{
				_buffTime = (int)(value * 60);
			}
		}

		public override bool PreAI(Projectile projectile)
		{
			if (_buffTime != -1)
			{
				_timeAlive++;
			}

			return true;
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (_buffTime != -1 && _timeAlive < _buffTime)
			{
				modifiers.FinalDamage += 0.3f;
			}
		}
	}
}