using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillAugments;

internal class Disperse : SkillAugment
{
	public const float AreaMult = 1.2f;
	public const float ManaMult = 1.3f;

	public override string Tooltip => base.Tooltip.FormatWith(MathUtils.Percent(AreaMult - 1), MathUtils.Percent(ManaMult - 1));

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= ManaMult;
		buff.AreaOfEffect *= AreaMult;
	}
}