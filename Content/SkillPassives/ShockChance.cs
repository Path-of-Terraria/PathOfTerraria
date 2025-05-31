using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

internal class ShockChance : SkillPassive
{
	public const float Chance = 0.02f;
	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(Round(Chance));

	public ShockChance(SkillTree tree) : base(tree)
	{
		MaxLevel = 3;
	}

	private static int Round(float value)
	{
		return (int)Math.Round(value * 100);
	}
}