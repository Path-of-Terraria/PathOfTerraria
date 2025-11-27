using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class SoulReaveMastery : Passive
{
	internal class SoulReaveNPC : GlobalNPC
	{
		internal class SoulReaveStack(int time, int who, int damage)
		{
			public int Time = time;
			public int Who = who;
			public int Damage = damage;
		}

		public override bool InstancePerEntity => true;

		internal readonly List<SoulReaveStack> Stacks = [];

		public override bool PreAI(NPC npc)
		{
			for (int i = 0; i < Stacks.Count; i++)
			{
				SoulReaveStack stack = Stacks[i];
				stack.Time--;

				if (stack.Time == 0)
				{
					npc.SimpleStrikeNPC(stack.Damage, 0);
					Player plr = Main.player[stack.Who];
					plr.Heal((int)(plr.statLifeMax2 * HealPercent / 100f));
				
					Stacks.RemoveAt(i);
					i--;
				}
			}

			return true;
		}
	}

	internal class SoulReavePlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.DamageType.CountsAsClass(DamageClass.Summon) && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SoulReaveMastery>(out float value)
				&& Main.rand.NextFloat() < value / 100f)
			{
				SoulReaveDebuff.Apply(target, 3 * 60, Player.whoAmI, damageDone);
			}
		}
	}

	public const float HealPercent = 3;
	public const float ReaveDamageProportion = 150;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, HealPercent, ReaveDamageProportion);
}