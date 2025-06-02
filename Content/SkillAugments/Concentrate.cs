using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillAugments;

internal class Concentrate : SkillAugment
{
	public const float DamageMult = 1.3f;
	public const float ManaMult = 1.35f;
	public const float AreaMult = 0.75f;

	public override string Tooltip => base.Tooltip.FormatWith(MathUtils.Percent(DamageMult - 1), MathUtils.Percent(ManaMult - 1), -MathUtils.Percent(AreaMult - 1));

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= ManaMult;
		buff.Damage *= DamageMult;
		buff.AreaOfEffect *= AreaMult;
	}
}