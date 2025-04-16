using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillSpecials;

internal class IceNova(SkillTree tree) : SkillSpecial(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/UI/SpecialFrame";
}