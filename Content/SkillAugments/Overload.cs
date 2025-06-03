using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillAugments;

internal class Overload : SkillAugment
{
	public const float ManaMult = 1.25f;
	public const float DamageMult = 1.25f;

	public override string Tooltip => base.Tooltip.FormatWith(MathUtils.Percent(ManaMult - 1), MathUtils.Percent(DamageMult - 1));

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= ManaMult;
		buff.Damage *= DamageMult;
	}
}