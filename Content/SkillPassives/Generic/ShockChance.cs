using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.Generic;

internal class ShockChance : SkillPassive
{
	public const float Chance = 0.05f;
	public override object[] TooltipArguments => [(Chance * 100).ToString()];

	public ShockChance(SkillTree tree) : base(tree)
	{
		MaxLevel = 3;
	}
}