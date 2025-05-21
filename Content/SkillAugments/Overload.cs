using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillAugments;

internal class Overload : SkillAugment
{
	public const float ManaMult = 1.25f;
	public const float DamageMult = 1.25f;

	public override string Tooltip => base.Tooltip.FormatWith(Round(ManaMult), Round(DamageMult));
	private static int Round(float value)
	{
		return (int)Math.Round((value - 1) * 100);
	}

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= ManaMult;
		buff.Damage *= DamageMult;
	}
}