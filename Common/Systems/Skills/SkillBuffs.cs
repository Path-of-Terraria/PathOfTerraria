using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Common.Systems.Skills;

/// <summary> Contains stat modifiers resulting from the <see cref="SkillPassive"/>s and <see cref="SkillAugment"/>s of a skill. </summary>
public class SkillBuff
{
	public StatModifier Damage = StatModifier.Default;
	public StatModifier ManaCost = StatModifier.Default;
}

internal static class SkillBuffMethods
{
	public static SkillBuff GetPower(this Skill skill)
	{
		var buff = new SkillBuff();

		SkillTree tree = skill.Tree;
		if (tree is null) //This skill has no tree
		{
			return buff;
		}

		foreach (Vector2 key in tree.Nodes.Keys)
		{
			if (tree.Nodes[key] is SkillPassive p)
			{
				p.PassiveEffects(ref buff);
			}
		}

		foreach (SkillAugment a in tree.Augments)
		{
			if (a is null)
			{
				continue;
			}

			a.AugmentEffects(ref buff);
		}

		return buff;
	}
}