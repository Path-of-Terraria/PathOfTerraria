using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.SkillPassives;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Skills;

public abstract class SkillTree : ILoadable
{
	public const int AugmentCount = 2;

	/// <summary> The currently viewed skill tree. </summary>
	internal static SkillTree Current;

	public static readonly Dictionary<Type, SkillTree> TypeToSkillTree = [];

	public List<SkillNode> Nodes = [];
	public SkillAugment[] Augments = new SkillAugment[AugmentCount];

	internal List<Edge> Edges = [];

	/// <summary> The indexes of unlocked augment slots. </summary>
	public bool[] Slots = new bool[AugmentCount];

	/// <summary> The number of points available for spending in this skill tree. </summary>
	public int Points;

	/// <summary> The current skill specialization of this tree. </summary>
	public SkillSpecial Specialization;

	public bool TryGetNode<T>(out T value) where T : SkillNode
	{
		T first = Nodes.FirstOrDefault(x => x is T) as T;
		if (first != default)
		{
			value = first;
			return true;
		}

		value = null;
		return false;
	}

	/// <summary> The Type of skill this tree belongs to, added to <see cref="TypeToSkillTree"/> during loading. </summary>
	public abstract Type ParentSkill { get; }

	/// <summary> Add skill tree elements by position here. </summary>
	public virtual void Populate() { }

	public virtual void SaveData(Skill skill, TagCompound tag)
	{
		string skillName = skill.Name;
		Dictionary<string, int> nameToLevel = [];

		foreach (Allocatable item in Nodes)
		{
			if (item is SkillPassive passive && item is not Anchor && passive.Level != 0)
			{
				nameToLevel.Add(passive.Name, passive.Level);
			}
		}

		tag["passives"] = nameToLevel.Keys.ToList(); //Save passives
		tag["levels"] = nameToLevel.Values.ToList();

		if (Specialization is not null)
		{
			tag["special"] = Specialization.Name;
		}

		tag[nameof(Slots)] = Slots.ToList(); //Save augments
		tag["augments"] = Augments.Where(x => x is not null).Select(x => x.Name).ToList();
	}

	public virtual void LoadData(Skill skill, TagCompound tag)
	{
		string skillName = skill.Name;

		IList<string> names = tag.GetList<string>("passives"); //Load passives
		IList<int> levels = tag.GetList<int>("levels");

		for (int i = 0; i < names.Count; i++)
		{
			((SkillPassive)Nodes.First(x => x.Name == names[i] && x is SkillPassive)).Level = levels[i];
		}

		var special = (SkillSpecial)Nodes.FirstOrDefault(x => x is SkillSpecial && x.Name == tag.GetString("special"));
		if (special != default)
		{
			Specialization = special;
		}

		if (tag.TryGet(nameof(Slots), out List<bool> list)) //Use TryGet because we don't want to assign an empty array
		{
			Slots = [.. list];
		}

		IList<string> augments = tag.GetList<string>("augments");

		int index = 0;
		foreach (string name in augments)
		{
			if (index >= Slots.Length || !Slots[index])
			{
				break;
			}

			if (!SkillAugment.LoadedAugments.ContainsKey(name))
			{
				continue;
			}

			Augments[index] = SkillAugment.LoadedAugments[name];
			index++;
		}
	}

	public void Load(Mod mod)
	{
		Populate();
		TypeToSkillTree.Add(ParentSkill, this);
	}

	public void Unload() { }
}