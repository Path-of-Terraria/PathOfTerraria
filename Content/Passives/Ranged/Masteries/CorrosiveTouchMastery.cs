using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Passives;

internal class CorrosiveTouchMastery : Passive
{
	internal class CorrosiveTouchPlayer : ModPlayer
	{
		private int _poisonedHits = 0;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.HasBuff<PoisonedDebuff>() && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<CorrosiveTouchMastery>(out float value))
			{
				_poisonedHits++;

				if (_poisonedHits >= value)
				{
					List<PoisonNPC.PoisonStack> stacks = target.GetGlobalNPC<PoisonNPC>().Stacks;

					if (stacks.Count <= 0)
					{
						return;
					}

					PoisonNPC.PoisonStack stack = Main.rand.Next(stacks);
					float totalDamage = stack.Time * stack.DamagePerTick / 60f;

					DoTFunctionality.ApplyDoT(target, (int)totalDamage, ref totalDamage, Color.Brown, Color.Red);
					stack.Time = 0;

					_poisonedHits = 0;
				}
			}
		}
	}
}