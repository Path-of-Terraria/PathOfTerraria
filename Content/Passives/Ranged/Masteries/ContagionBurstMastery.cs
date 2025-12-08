using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using System.Collections.Generic;
using System.Reflection;

namespace PathOfTerraria.Content.Passives;

internal class ContagionBurstMastery : Passive
{
	internal class ContagionBurstPlayer : ModPlayer
	{
		private delegate void hook_OnHitNPC(Player player, NPC npc, in NPC.HitInfo hit, int damageDone);

		public override void Load()
		{
			// We need to apply this after the current NPC takes debuffs on OnHitNPC.
			// This makes it feel more consistent for the player, as an NPC would have the 'visible' amount of stacks applied.
			MonoModHooks.Add(typeof(PlayerLoader).GetMethod(nameof(PlayerLoader.OnHitNPC), BindingFlags.Static | BindingFlags.Public), LateOnHitNPC);
		}

		private static void LateOnHitNPC(hook_OnHitNPC orig, Player player, NPC npc, in NPC.HitInfo hit, int damageDone)
		{
			orig(player, npc, in hit, damageDone);

			if (player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ContagionBurstMastery>(out float value) 
				&& npc.GetGlobalNPC<PoisonNPC>().Stacks is { Length: >0 } stacks && npc.life <= 0)
			{
				int stacksToAdd = (int)MathF.Ceiling(stacks.Length * value / 100f);

				foreach (NPC other in Main.ActiveNPCs)
				{
					if (other.CanBeChasedBy() && other.whoAmI != npc.whoAmI && other.DistanceSQ(npc.Center) < PoTMod.NearbyDistanceSq)
					{
						PoisonNPC otherPoison = other.GetGlobalNPC<PoisonNPC>();
						ReadOnlySpan<PoisonNPC.PoisonStack> list = otherPoison.Stacks;
						int time = 0;

						for (int i = 0; i < stacksToAdd; ++i)
						{
							PoisonNPC.PoisonStack stack = stacks[i];
							otherPoison.AddStack(stack);
							time = Math.Max(time, stack.MaxTime);
						}

						other.AddBuff(ModContent.BuffType<PoisonedDebuff>(), time);
					}
				}
			}
		}
	}

	public override void BuffPlayer(Player player)
	{
		player.GetAttackSpeed(DamageClass.Ranged) += Value / 100f;
	}
}