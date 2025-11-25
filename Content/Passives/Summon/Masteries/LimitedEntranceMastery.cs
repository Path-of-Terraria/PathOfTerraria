using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class LimitedEntranceMastery : Passive
{
	internal class LimitedEntranceProjectile : GlobalProjectile 
	{
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;

		private int _timeAlive = 0;
		private int _buffTime = -1;

		//public override GlobalProjectile Clone(Projectile from, Projectile to)
		//{
		//	var lim = (LimitedEntranceProjectile)base.Clone(from, to);
		//	lim._buffTime = _buffTime;
		//	lim._timeAlive = _timeAlive;
		//	return lim;
		//}

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_Parent { Entity: Player plr } && HasNode(plr, out float value))
			{
				_buffTime = (int)(value * 60);
			}
			else if (source is EntitySource_Parent { Entity: Projectile proj } && proj.TryGetOwner(out Player projOwner) && HasNode(projOwner, out float value2))
			{
				_buffTime = (int)(value2 * 60);
			}
			else if (source is EntitySource_ItemUse_WithAmmo { Player: Player plr2 } && HasNode(plr2, out float value3))
			{
				_buffTime = (int)(value3 * 60);
			}
		}

		private static bool HasNode(Player plr, out float value)
		{
			return plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<LimitedEntranceMastery>(out value);
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