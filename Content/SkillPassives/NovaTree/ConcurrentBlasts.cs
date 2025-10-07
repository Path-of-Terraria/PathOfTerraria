using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.NovaTree;

internal class ConcurrentBlasts(SkillTree tree) : SkillPassive(tree)
{
	/// <summary> A damage multiplier applied to targets based on <see cref="ConcurrentNPC.Vulnerable"/>. </summary>
	public const float BonusDamage = 1.15f;
	public override object[] TooltipArguments => [MathUtils.Percent(BonusDamage - 1)];
}
