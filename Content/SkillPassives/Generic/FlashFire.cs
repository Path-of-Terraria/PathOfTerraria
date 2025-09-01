using Humanizer;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.Generic;

internal class FlashFire(SkillTree tree) : SkillPassive(tree)
{
	public const float DamageReduction = 0.35f;
	public override string DisplayTooltip => base.DisplayTooltip.FormatWith(MathUtils.Percent(DamageReduction));

	public override void PassiveEffects(ref SkillBuff buff)
	{
		buff.Damage *= 1f - DamageReduction;
		buff.Cooldown *= 0.5f;
	}
}