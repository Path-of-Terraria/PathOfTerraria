using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class PoisonedRangedAttacksPassive : Passive
{
	internal class PoisonedRangerPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.DamageType.CountsAsClass(DamageClass.Ranged) && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PoisonedRangedAttacksPassive>(out float value)
				&& Main.rand.NextFloat() < value / 100f)
			{
				PoisonedDebuff.Apply(target, 5 * 60, Player);
			}
		}
	}
}