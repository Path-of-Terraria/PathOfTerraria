using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives;

internal class IgniteChance : SkillPassive
{
	public const float Chance = 0.02f;
	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(MathUtils.Percent(Chance));

	public IgniteChance(SkillTree tree) : base(tree)
	{
		MaxLevel = 3;
	}
}