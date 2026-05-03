using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class BloodbenderMastery : Passive
{
	internal class BloodbenderPlayer : ModPlayer
	{
		private int healCooldown = 0;
		
		public override void PostUpdate()
		{
			if (healCooldown > 0)
			{
				healCooldown--;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.Crit && healCooldown <= 0 && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BloodbenderMastery>(out float value) 
			    && target.TryGetGlobalNPC(out BleedDebuffNPC bleed) && bleed.Stacks is { Length: > 0 and int length })
			{
				Player.Heal((int)(damageDone * value / 100f * length));
				healCooldown = 30;
			}
		}
	}
}