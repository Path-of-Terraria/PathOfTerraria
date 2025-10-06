using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.RainOfArrowsPassives;

internal class ColdBlast(SkillTree tree) : SkillPassive(tree)
{
	// Duration in seconds, % of damage converted
	public override object[] TooltipArguments => ["2", "100"];
}

internal class CreepingVines(SkillTree tree) : SkillPassive(tree);

internal class ConcussiveBurst(SkillTree tree) : SkillPassive(tree);

internal class FesteringSpores(SkillTree tree) : SkillPassive(tree);

internal class FungalSpread(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["10"];
}

internal class Ghostfire(SkillTree tree) : SkillPassive(tree);

internal class LingeringPoison(SkillTree tree) : SkillPassive(tree);

internal class Megatoxin(SkillTree tree) : SkillPassive(tree);

internal class MoldColony(SkillTree tree) : SkillPassive(tree);

internal class PowerfulSmog(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["1"];
}

internal class Quickload(SkillTree tree) : SkillPassive(tree);

internal class SharpenedTips(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["10"];
}

internal class ShatteringArrows(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["70"];
}

internal class SlicingShrapnel(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["3"];
}

internal class TargetLock(SkillTree tree) : SkillPassive(tree);