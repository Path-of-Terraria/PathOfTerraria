using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class RangerMeleeChanceToBleedPassive : Passive
{
	internal class RangerMeleeChanceToBleedPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.DamageType.CountsAsClass(DamageClass.Melee) || hit.DamageType.CountsAsClass(DamageClass.Ranged))
			{
				float chance = Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<RangerMeleeChanceToBleedPassive>();

				//if (Main.rand.NextFloat() < chance / 100f)
				{
					BleedDebuff.Apply(Player, target, damageDone);
				}
			}
		}
	}
}
