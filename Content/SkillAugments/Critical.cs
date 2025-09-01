using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillAugments;

internal class Critical : SkillAugment
{
	public const float CritChanceMult = 1.50f;
	public const float ManaMult = 1.30f;

	public override string Tooltip => base.Tooltip.FormatWith(MathUtils.Percent(CritChanceMult - 1), MathUtils.Percent(ManaMult - 1));

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.CritChance += 2;
		buff.CritChance *= CritChanceMult;
		buff.ManaCost *= ManaMult;
	}
}