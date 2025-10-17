using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Skills.Summon;

namespace PathOfTerraria.Content.SkillAugments;

internal class Command : SkillAugment
{
	public const float DamageMult = 1.30f;

	public override string Tooltip => base.Tooltip.FormatWith(MathUtils.Percent(DamageMult - 1));

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.Damage *= DamageMult;
	}

	public override bool CanBeApplied(Skill skill)
	{
		return skill is SummonSkill || skill is PestSwarm;
	}
}