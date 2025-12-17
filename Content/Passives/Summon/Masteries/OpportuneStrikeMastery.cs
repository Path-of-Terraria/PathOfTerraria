using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class OpportuneStrikeMastery : Passive
{
	internal class OpportuneStrikePlayer : ModPlayer
	{
		private int _cooldown = 0;

		public override void ResetEffects()
		{
			_cooldown--;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.Crit && item.CountsAsClass(DamageClass.Summon) && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<OpportuneStrikeMastery>(out float value))
			{
				ReduceSkillCooldowns(value);
			}
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.Crit && proj.minion && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<OpportuneStrikeMastery>(out float value))
			{
				ReduceSkillCooldowns(value);
			}
		}

		private void ReduceSkillCooldowns(float value)
		{
			if (_cooldown > 0)
			{
				return;
			}

			SkillCombatPlayer plr = Player.GetModPlayer<SkillCombatPlayer>();

			foreach (Skill skill in plr.HotbarSkills)
			{
				if (skill != null && skill.Tags().HasFlag(SkillTags.Summon))
				{
					skill.Cooldown = (int)(skill.Cooldown * (1 - value / 100f));
				}
			}

			_cooldown = 60;
		}
	}
}