using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives;

internal class ShockChance : SkillPassive
{
	public const float Chance = 0.02f;
	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(MathUtils.Percent(Chance));

	public ShockChance(SkillTree tree) : base(tree)
	{
		MaxLevel = 3;
	}
}