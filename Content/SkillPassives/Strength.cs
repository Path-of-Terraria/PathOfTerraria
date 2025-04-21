using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

internal class Strength(SkillTree tree) : SkillPassive(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/UI/PassiveFrameTiny";

	public override void PassiveEffects(ref SkillBuff buff)
	{
		base.PassiveEffects(ref buff);
	}
}