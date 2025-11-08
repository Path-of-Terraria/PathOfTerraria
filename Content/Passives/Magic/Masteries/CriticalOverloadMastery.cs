using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

#nullable enable

internal class CriticalOverloadMastery : Passive
{
	internal class CriticalOverloadPlayer : ModPlayer
	{
		private int missedCrits = 0;

		public override ModPlayer Clone(Player newEntity)
		{
			CriticalOverloadPlayer plr = (CriticalOverloadPlayer)base.Clone(newEntity);
			plr.missedCrits = missedCrits;
			return plr;
		}

		public override void ResetEffects()
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<CriticalOverloadMastery>(out float value))
			{
				Player.GetCritChance(DamageClass.Generic) += missedCrits * value / 100f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.Crit)
			{
				missedCrits = 0;
			}
			else
			{
				missedCrits++;
			}
		}
	}
}
