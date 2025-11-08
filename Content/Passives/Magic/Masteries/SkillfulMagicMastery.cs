using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class SkillfulMagicMastery : Passive
{
	internal class SkillfulMagicPlayer : ModPlayer, SkillHooks.IOnUseSkillPlayer
	{
		public void OnUseSkill(Skill skill)
		{
			Player.AddBuff(ModContent.BuffType<SkillfulBoostBuff>(), 3 * 60);
		}

		public override void PostUpdateRunSpeeds()
		{
			if (Player.HasBuff<SkillfulBoostBuff>())
			{
				Player.maxRunSpeed *= 1.5f;
			}
		}
	}
}
