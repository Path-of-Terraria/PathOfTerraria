using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Skills.Ranged;

namespace PathOfTerraria.Content.SkillAugments;

internal class Extend : SkillAugment
{
	public const float DurationIncrease = 0.3f;
	public override string Tooltip => base.Tooltip.FormatWith(MathUtils.Percent(DurationIncrease));

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.Duration *= (1f + DurationIncrease);
	}

	public override bool CanBeApplied(Skill skill)
	{
		return skill.Duration > 0 || skill is RainOfArrows; //Make a special case for Rain of Arrows
	}
}