using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class LongRangePassive : Passive
{
	internal class LongRangeGlobalNPC : GlobalNPC
	{
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.TryGetOwner(out Player plr) && projectile.DistanceSQ(npc.Center) < PoTMod.NearbyDistanceSq
				&& plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<LongRangePassive>(out float value))
			{
				modifiers.FinalDamage += value / 100f;
			}
		}
	}
}