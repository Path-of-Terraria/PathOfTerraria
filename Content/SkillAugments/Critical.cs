using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillAugments;

internal class Critical : SkillAugment
{
	public const float CritChanceMult = 1.50f;
	public const float ManaMult = 1.30f;

	public override string Tooltip => base.Tooltip.FormatWith(Round(CritChanceMult), Round(ManaMult));
	private static int Round(float value)
	{
		return (int)Math.Round((value - 1) * 100);
	}

	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.CritChance += 2;
		buff.CritChance *= CritChanceMult;
		buff.ManaCost *= ManaMult;
	}
}