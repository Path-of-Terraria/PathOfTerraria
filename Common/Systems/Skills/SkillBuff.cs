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
	public StatModifier Duration = StatModifier.Default;

	public void Reset()
	{
		Damage = StatModifier.Default;
		ManaCost = StatModifier.Default;
		AreaOfEffect = StatModifier.Default;
		CritChance = StatModifier.Default;
		Cooldown = StatModifier.Default;
		Duration = StatModifier.Default;
	}

	public static SkillBuff operator *(SkillBuff self, SkillBuff other)
	{
		return new SkillBuff()
		{
			Damage = self.Damage.CombineWith(other.Damage),
			ManaCost = self.ManaCost.CombineWith(other.ManaCost),
			AreaOfEffect = self.AreaOfEffect.CombineWith(other.AreaOfEffect),
			CritChance = self.CritChance.CombineWith(other.CritChance),
			Cooldown = self.Cooldown.CombineWith(other.Cooldown),
			Duration = self.Duration.CombineWith(other.Duration),
		};
	}
}