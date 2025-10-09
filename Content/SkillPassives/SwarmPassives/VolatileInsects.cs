using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

internal class VolatileInsects(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["300"];
}
