using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillAugments;

internal class Disperse : SkillAugment
{
	public const float AreaMult = 1.2f;
	public const float ManaMult = 1.3f;

	public override string Tooltip => base.Tooltip.FormatWith(Round(AreaMult), Round(ManaMult));
	private static int Round(float value)
	{
		return (int)Math.Round((value - 1) * 100);
	}

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= ManaMult;
		buff.AreaOfEffect *= AreaMult;
	}
}