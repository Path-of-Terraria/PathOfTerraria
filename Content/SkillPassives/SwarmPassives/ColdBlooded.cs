using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

internal class ColdBlooded(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["1"];
}
