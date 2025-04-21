using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

public class Anchor(SkillTree tree) : SkillPassive(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/AnchorPassive";
}