using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

internal class ShockChance(SkillTree tree) : SkillPassive(tree)
{
	public const float Chance = 0.02f;
	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(Round(Chance));

	private static int Round(float value)
	{
		return (int)Math.Round(value * 100);
	}
}