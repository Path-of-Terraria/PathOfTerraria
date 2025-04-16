using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

public class SkillPassiveAnchor(SkillTree tree) : SkillPassive(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/AnchorPassive";
}