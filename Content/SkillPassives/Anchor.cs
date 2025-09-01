using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives;

public sealed class Anchor(SkillTree tree) : SkillPassive(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/AnchorPassive";

	public override bool CanAllocate(Player player)
	{
		return false;
	}

	public override bool CanDeallocate(Player player)
	{
		return false;
	}
}