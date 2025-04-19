using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.SkillPassives;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Skills;

public abstract class SkillTree : ILoadable
{
	internal readonly record struct Edge(Allocatable Start, Allocatable End)
	{
		public readonly Allocatable Start = Start;
		public readonly Allocatable End = End;

		public bool Contains(Allocatable p)
		{
			return p == Start || p == End;
		}

		/// <summary> Assuming that p is either start or end - Contains returned true. </summary>
		public Allocatable Other(Allocatable p)
		{
			return p == Start ? End : Start;
		}
	}

	public static readonly Dictionary<Type, SkillTree> TypeToSkillTree = [];

	public Dictionary<Vector2, Allocatable> Allocatables = [];
	public SkillAugment[] Augments = new SkillAugment[3]; 

	internal List<Edge> Edges = [];

	/// <summary> The number of points available for spending in this skill tree. </summary>
	public int Points;
	/// <summary> The current skill specialization of this tree. </summary>
	public SkillSpecial Specialization;

	public Vector2 Point(Allocatable a)
	{
		foreach (Vector2 key in Allocatables.Keys)
		{
			if (Allocatables[key] == a)
			{
				return key;
			}
		}

		return default;
	}

	public abstract Type ParentSkill { get; }

	/// <summary> Add skill tree elements by position here. </summary>
	public virtual void Populate() { }

	public virtual void SaveData(Skill skill, TagCompound tag)
	{
		string skillName = skill.Name;
		Dictionary<string, int> nameToLevel = [];

		foreach (Allocatable item in Allocatables.Values)
		{
			if (item is SkillPassive passive && item is not SkillPassiveAnchor && passive.Level != 0)
			{
				nameToLevel.Add(passive.Name, passive.Level);
			}
		}

		tag["names"] = nameToLevel.Keys.ToList();
		tag["levels"] = nameToLevel.Values.ToList();
		tag["special"] = Specialization.Name;
	}

	public virtual void LoadData(Skill skill, TagCompound tag)
	{
		string skillName = skill.Name;

		IList<string> names = tag.GetList<string>("names");
		IList<int> levels = tag.GetList<int>("levels");

		for (int i = 0; i < names.Count; i++)
		{
			((SkillPassive)Allocatables.Values.First(x => x.Name == names[i] && x is SkillPassive)).Level = levels[i];
		}

		Specialization = (SkillSpecial)Allocatables.Values.First(x => x is SkillSpecial && x.Name == tag.GetString("special"));
	}

	public void Load(Mod mod)
	{
		Populate();
		TypeToSkillTree.Add(ParentSkill, this);
	}

	public void Unload() { }
}