using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

internal class Overload : SkillAugment
{
	public override void AugmentEffects(ref SkillBuff buff)
	{
		buff.ManaCost *= 1.25f;
		buff.Damage *= 1.25f;
	}
}