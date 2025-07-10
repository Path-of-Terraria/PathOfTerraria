using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.Generic;

internal class Efficiency : SkillPassive
{
	public const float Chance = 0.02f;
	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(MathUtils.Percent(Chance));

	public Efficiency(SkillTree tree) : base(tree)
	{
		MaxLevel = 3;
	}

	public override void PassiveEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= 1f - 0.02f * Level;
	}
}