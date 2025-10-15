using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.Generic;

internal class ShockChance : SkillPassive
{
	public const float Chance = 0.02f;
	public override object[] TooltipArguments => ["5"];

	public ShockChance(SkillTree tree) : base(tree)
	{
		MaxLevel = 3;
	}
}