using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.SkillPassives;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Skills;

public abstract class SkillTree : ILoadable
{
	public const int DefaultAugmentCount = 2;

	public record struct PackedAugment(SkillAugment Augment, bool Unlocked);

	/// <summary> The currently viewed skill tree. </summary>
	internal static SkillTree Current;

	public static readonly Dictionary<Type, SkillTree> TypeToSkillTree = [];

	/// <summary> Stores skill augments and whether they are unlocked. </summary>
	public readonly List<PackedAugment> Augments = [];

	public List<SkillNode> Nodes = [];

	internal List<Edge> Edges = [];

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

	/// <summary> Adds a list of connected <see cref="SkillNode"/>s pointing to the first element. Used to help build skill trees in <see cref="Populate"/>. </summary>
	public void AddNodes(params SkillNode[] list)
	{
		for (int i = 0; i < list.Length; i++)
		{
			SkillNode c = list[i];

			if (!Nodes.Contains(c))
			{
				Nodes.Add(c);

				// Invokes each so we can register each value automatically as both call GetOrRegister
				_ = c.DisplayName;
				_ = c.DisplayTooltip;
			}

			if (i != 0)
			{
				Edges.Add(new(c, list[0], EdgeFlags.None));
			}
		}
	}

	public virtual void SaveData(Skill skill, TagCompound tag)
	{
		string skillName = skill.Name;
		Dictionary<string, int> nameToLevel = [];

		foreach (SkillNode item in Nodes)
		{
			if (item is not Anchor && item.Level != 0)
			{
				nameToLevel.Add(item.Name, item.Level);
			}
		}

		tag["passives"] = nameToLevel.Keys.ToList(); //Save passives
		tag["levels"] = nameToLevel.Values.ToList();

		if (Specialization is not null)
		{
			tag["special"] = Specialization.Name;
		}

		var augments = Augments.ToList();
		tag["augmentNames"] = augments.Select(x => x.Augment?.Name ?? "null").ToList();
		tag["augmentUnlocks"] = augments.Select(x => x.Unlocked).ToList();
	}

	public virtual void LoadData(Skill skill, TagCompound tag, Player loadingPlayer)
	{
		// Data must be delayed as SkillTrees are singletons.
		// This means multiple players modifying one tree would just override everyone but the last player's information.
		// We delay it here so it only loads on world load, avoiding the issue.
		loadingPlayer.GetModPlayer<SkillTreePlayer>().AddCache(new SkillTreePlayer.CachedSkillTreeData(this, skill, tag));
	}

	/// <summary>
	/// Loads data for a skill from a tag.
	/// </summary>
	internal void LoadDelayedData(Skill skill, TagCompound tag)
	{
		string skillName = skill.Name;

		IList<string> names = tag.GetList<string>("passives"); //Load passives
		IList<int> levels = tag.GetList<int>("levels");

		for (int i = 0; i < names.Count; i++)
		{
			Nodes.First(x => x.Name == names[i]).Level = levels[i];
		}

		var special = (SkillSpecial)Nodes.FirstOrDefault(x => x is SkillSpecial && x.Name == tag.GetString("special"));
		Specialization = special;

		IList<string> augmentNames = tag.GetList<string>("augmentNames");
		IList<bool> augmentUnlocked = tag.GetList<bool>("augmentUnlocks");
		int count = Math.Min(augmentUnlocked.Count, DefaultAugmentCount);

		for (int c = 0; c < count; c++)
		{
			SkillAugment a = (c < augmentNames.Count && augmentNames[c] != "null") ? SkillAugment.LoadedAugments[augmentNames[c]] : null;
			bool u = c < augmentUnlocked.Count && augmentUnlocked[c];

			Augments.Add(new(a, u));
		}
	}

	public void Load(Mod mod)
	{
		Populate();
		TypeToSkillTree.Add(ParentSkill, this);
	}

	public void Unload() { }
}
