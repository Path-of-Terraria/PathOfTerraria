using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.NovaTree;

internal class ThunderClaps : SkillPassive
{
	public override object[] TooltipArguments => ["5"];
		
	public ThunderClaps(SkillTree tree) : base(tree)
	{
		MaxLevel = 2;
	}
}
