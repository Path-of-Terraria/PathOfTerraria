using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.UI.SkillsTree;

namespace PathOfTerraria.Content.SkillAugments;

public sealed class SpareSlot(SkillTree tree) : SkillNode(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/UI/AugmentFrame";
	public override string DisplayName => string.Empty;
	public override string DisplayTooltip => string.Empty;

	public override Vector2 Size => new(AugmentSlotElement.SquareSize);
}