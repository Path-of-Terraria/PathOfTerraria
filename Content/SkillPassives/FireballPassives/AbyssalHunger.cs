using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class AbyssalHunger(SkillTree tree) : SkillPassive(tree)
{
	public const float Recovery = 0.05f;
	public const float Chance = 0.1f;

	public override object[] TooltipArguments => [$"{Recovery * 100:#0}", $"{Chance * 100:#0}"];
}
