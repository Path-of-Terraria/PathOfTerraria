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

	/// <summary> The currently viewed skill tree. </summary>
	internal static SkillTree Current;

	public static readonly Dictionary<Type, SkillTree> TypeToSkillTree = [];

	public Dictionary<Vector2, SkillNode> Nodes = [];
	public SkillAugment[] Augments = new SkillAugment[3]; 

	internal List<Edge> Edges = [];

	/// <summary> The number of points available for spending in this skill tree. </summary>
	public int Points;
	/// <summary> The current skill specialization of this tree. </summary>
	public SkillSpecial Specialization;

	public Vector2 Point(Allocatable a)
	{
		foreach (Vector2 key in Nodes.Keys)
		{
			if (Nodes[key] == a)
			{
				return key;
			}
		}

		return default;
	}

	/// <summary> The Type of skill this tree belongs to, added to <see cref="TypeToSkillTree"/> during loading. </summary>
	public abstract Type ParentSkill { get; }

	/// <summary> Add skill tree elements by position here. </summary>
	public virtual void Populate() { }

	public virtual void SaveData(Skill skill, TagCompound tag)
	{
		string skillName = skill.Name;
		Dictionary<string, int> nameToLevel = [];

		foreach (Allocatable item in Nodes.Values)
		{
			if (item is SkillPassive passive && item is not Anchor && passive.Level != 0)
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
			((SkillPassive)Nodes.Values.First(x => x.Name == names[i] && x is SkillPassive)).Level = levels[i];
		}

		Specialization = (SkillSpecial)Nodes.Values.First(x => x is SkillSpecial && x.Name == tag.GetString("special"));
	}

	public void Load(Mod mod)
	{
		Populate();
		TypeToSkillTree.Add(ParentSkill, this);
	}

	public void Unload() { }
}

internal class SkillTreePlayer : ModPlayer
{
	private HashSet<string> AcquiredAugments = [];

	/// <summary> Unlocks the given skill augment for this player. </summary>
	public void UnlockAugment(SkillAugment augment)
	{
		AcquiredAugments.Add(augment.Name);
	}

	public bool Unlocked(SkillAugment augment)
	{
		return AcquiredAugments.Contains(augment.Name);
	}

	public override void SaveData(TagCompound tag)
	{
		tag[nameof(AcquiredAugments)] = AcquiredAugments.ToList();
	}

	public override void LoadData(TagCompound tag)
	{
		AcquiredAugments = tag.GetList<string>(nameof(AcquiredAugments)).ToHashSet();
	}
}