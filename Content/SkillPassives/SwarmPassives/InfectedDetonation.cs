using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

internal class InfectedDetonation(SkillTree tree) : SkillPassive(tree)
{
	// % chance to explode, % of max health used as damage
	public override object[] TooltipArguments => ["10", "10"];
}
