using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Common.Systems.Skills;

/// <summary> Contains stat modifiers resulting from the <see cref="SkillPassive"/>s and <see cref="SkillAugment"/>s of a skill. </summary>
public class SkillBuff
{
	public StatModifier Damage = StatModifier.Default;
	public StatModifier ManaCost = StatModifier.Default;
	public StatModifier AreaOfEffect = StatModifier.Default;
	public StatModifier CritChance = StatModifier.Default;
	public StatModifier Cooldown = StatModifier.Default;
}