using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class AdditionalPyres(SkillTree tree) : SkillPassive(tree)
{
	public override void PassiveEffects(ref SkillBuff buff)
	{
		buff.Cooldown *= MathF.Pow(0.85f, Level);
	}
}
