using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class BlastForce : SkillPassive
{
	public const float Increase = 0.05f;
	public override string DisplayTooltip => string.Format(base.DisplayTooltip, MathUtils.Percent(Increase));

	public BlastForce(SkillTree tree) : base(tree)
	{
		MaxLevel = 2;
	}
}