using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillAugments;

internal class Duplicate : SkillAugment
{
	public const float DamageReduction = 0.3f;
	public override string Tooltip => base.Tooltip.FormatWith(MathUtils.Percent(DamageReduction));

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.Damage *= (1f - DamageReduction);
	}

	public override bool CanBeApplied(Skill skill)
	{
		return skill is SummonSkill;
	}
}