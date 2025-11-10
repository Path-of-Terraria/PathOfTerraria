using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class ArcaneSurgeMastery : Passive
{
	internal class ArcaneSurgePlayer : ModPlayer, SkillHooks.IOnUseSkillPlayer
	{
		internal readonly List<int> TrackedTimers = new(5);

		public void OnUseSkill(Skill skill)
		{
			AddTimer();
		}

		internal void AddTimer()
		{
			TrackedTimers.Add(ArcaneSurgeMaxTime);

			if (TrackedTimers.Count >= 5)
			{
				Player.AddBuff(ModContent.BuffType<ArcaneSurgeBuff>(), 6 * 60);
			}
		}

		public override void PostUpdate()
		{
			for (int i = 0; i < TrackedTimers.Count; ++i)
			{
				TrackedTimers[i]--;
			}

			TrackedTimers.RemoveAll(x => x <= 0);
		}
	}

	internal class ArcaneSurgeItem : GlobalItem
	{
		public override bool? UseItem(Item item, Player player)
		{
			if (item.CountsAsClass(DamageClass.Magic))
			{
				player.GetModPlayer<ArcaneSurgePlayer>().AddTimer();
			}

			return null;
		}
	}

	const int ArcaneSurgeMaxTime = 4 * 60;

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.FreezeEffectiveness += Value / 100f;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ChilledEffectiveness += Value / 50f;
	}
}
