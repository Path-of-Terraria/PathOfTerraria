using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillAugments;

internal class Concentrate : SkillAugment
{
	public const float DamageMult = 1.3f;
	public const float ManaMult = 1.35f;
	public const float AreaMult = 0.75f;

	public override string Tooltip => base.Tooltip.FormatWith(Round(DamageMult), Round(ManaMult), -Round(AreaMult));
	private static int Round(float value)
	{
		return (int)Math.Round((value - 1) * 100);
	}

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= ManaMult;
		buff.Damage *= DamageMult;
		buff.AreaOfEffect *= AreaMult;
	}
}