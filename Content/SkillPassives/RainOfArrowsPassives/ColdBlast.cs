using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.RainOfArrowsPassives;

internal class ColdBlast(SkillTree tree) : SkillPassive(tree)
{
	// Duration in seconds, % of damage converted
	public override object[] TooltipArguments => ["2", "100"];
}
