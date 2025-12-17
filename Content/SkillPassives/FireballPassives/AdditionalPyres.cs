using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class AdditionalPyres(SkillTree tree) : SkillPassive(tree)
{
	public const float DurationModifier = 0.15f;

	public override object[] TooltipArguments => [$"{DurationModifier * 100:#0}"];

	public override void PassiveEffects(ref SkillBuff buff)
	{
		buff.Cooldown *= MathF.Pow(1 - DurationModifier, Level);
	}
}
