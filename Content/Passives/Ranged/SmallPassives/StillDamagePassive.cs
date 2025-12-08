using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class StillDamagePassive : Passive
{
	internal class StillPlayer : GlobalNPC
	{
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			if (projectile.TryGetOwner(out Player plr) && plr.velocity.LengthSquared() < 0.1f
				&& plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StillDamagePassive>(out float value))
			{
				modifiers.FinalDamage += value / 100f;
			}
		}
	}
}