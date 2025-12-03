using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class ChaoticReverberationMastery : Passive
{
	public class ChaoticReverbPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life >= 0 && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ChaoticReverberationMastery>(out float value) 
				&& !ElementalPlayer.ApplyingElementalDamage)
			{
				ElementalReverberationBuff.Apply(Player, target, 4 * 60, (int)(damageDone * value / 100f), ElementType.Chaos);
			}
		}
	}
}