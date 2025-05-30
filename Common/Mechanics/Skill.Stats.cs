using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Common.Mechanics;

public abstract partial class Skill
{
	public SkillBuff Stats = new();

	/// <summary> The final mana cost of this skill affected by modifications. </summary>
	public int TotalManaCost => (int)Stats.ManaCost.ApplyTo(ManaCost);

	/// <summary> Shorthand for applying to <see cref="SkillBuff.Damage"/>. </summary>
	public int GetTotalDamage(float apply)
	{
		return (int)Stats.Damage.ApplyTo(apply);
	}
	/// <summary> Shorthand for applying to <see cref="SkillBuff.AreaOfEffect"/>. </summary>
	public int GetTotalAreaOfEffect(float apply)
	{
		return (int)Stats.AreaOfEffect.ApplyTo(apply);
	}
	/// <summary> Shorthand for applying to <see cref="SkillBuff.CritChance"/>. </summary>
	public int GetTotalCritChance(float apply)
	{
		return (int)Stats.CritChance.ApplyTo(apply);
	}

	/// <summary> Recalculates <see cref="Stats"/>. </summary>
	/// <returns> The updated instance. </returns>
	public SkillBuff RecalculateStats()
	{
		var stats = new SkillBuff();

		SkillTree tree = Tree;
		if (tree is null) //This skill has no tree
		{
			return stats;
		}

		foreach (SkillNode node in tree.Nodes)
		{
			if (node is SkillPassive p)
			{
				p.PassiveEffects(ref stats);
			}
		}

		foreach (SkillAugment a in tree.Augments)
		{
			if (a is null)
			{
				continue;
			}

			a.AugmentEffects(ref stats);
		}

		return Stats = stats;
	}
}