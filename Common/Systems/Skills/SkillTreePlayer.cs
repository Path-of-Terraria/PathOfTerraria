using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Skills;

/// <summary>
/// Used to delay loading trees, and to sync skill tree information.<br/>
/// Most of the values have shorthand in <see cref="EntityExtensions"/>, such as <see cref="EntityExtensions.GetPassiveStrength{TTree, TPassive}(Player)"/>.
/// </summary>
internal class SkillTreePlayer : ModPlayer
{
	public readonly record struct CachedSkillTreeData(SkillTree Instance, Skill Skill, TagCompound Tag);

	internal readonly Dictionary<Type, SkillSpecial> SpecializationsBySkill = [];

	private readonly List<CachedSkillTreeData> cachedData = [];
	private readonly Dictionary<SkillTree, Dictionary<Type, int>> TotalLevelByTypeByTree = [];

	public void AddCache(CachedSkillTreeData cache)
	{
		cachedData.Add(cache);
	}

	public override void OnEnterWorld()
	{
		foreach (CachedSkillTreeData cache in cachedData)
		{
			cache.Instance.LoadDelayedData(cache.Skill, cache.Tag);
		}

		cachedData.Clear();

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			// Tell everyone what I have
			SyncAllPassives();
			SyncAllSpecializations();
			 
			// and ask everyone what they have
			RequestOtherSkillSpecializationHandler.Send((byte)Player.whoAmI);
			RequestOtherSkillPassivesHandler.Send((byte)Player.whoAmI);
		}
	}

	internal void SyncAllSpecializations()
	{
		foreach (KeyValuePair<Type, SkillSpecial> pair in SpecializationsBySkill)
		{
			SyncSkillSpecializationHandler.Send((byte)Player.whoAmI, pair.Key.FullName, pair.Value.GetType().FullName);
		}
	}

	/// <summary>
	/// Sends all passive values to the server & every other client.
	/// </summary>
	internal void SyncAllPassives()
	{
		foreach (SkillTree tree in TotalLevelByTypeByTree.Keys)
		{
			foreach (KeyValuePair<Type, int> pair in TotalLevelByTypeByTree[tree])
			{
				SkillPassiveValueHandler.Send((byte)Player.whoAmI, tree.GetType().FullName, pair.Key.FullName, (byte)pair.Value);
			}
		}
	}

	/// <summary>
	/// Sets the specialization for the given skill type. Used for syncing.
	/// </summary>
	/// <param name="type">Skill type.</param>
	/// <param name="spec">Specialization to set.</param>
	/// <param name="sync">Whether to sync this value setting or not.</param>
	internal void SetSpecializationForSkill(Type type, SkillSpecial spec, bool sync = true)
	{
		if (spec is null)
		{
			SpecializationsBySkill.Remove(type);
		}
		else
		{
			SpecializationsBySkill.Add(type, spec);
		}

		if (Main.netMode == NetmodeID.MultiplayerClient && sync)
		{
			SyncSkillSpecializationHandler.Send((byte)Player.whoAmI, type.FullName, spec.GetType().FullName);
		}
	}

	/// <summary>
	/// Modifies the stored value for a passive, per tree and per node.
	/// </summary>
	/// <param name="tree">The tree being modified.</param>
	/// <param name="nodeType">The node value being modified.</param>
	/// <param name="levelAdjustment">If <paramref name="set"/> is true, the final value to use. Otherwise, the value to add to the stored value.</param>
	/// <param name="sync">Whether this should run <see cref="SkillPassiveValueHandler.Send(byte, string, string, byte, bool)"/> or not.</param>
	/// <param name="set">Whether this overrides or adds to the stored value.</param>
	internal void ModifyPassive(SkillTree tree, Type nodeType, int levelAdjustment, bool sync = true, bool set = false)
	{
		TotalLevelByTypeByTree.TryAdd(tree, []);
		Dictionary<Type, int> levelByType = TotalLevelByTypeByTree[tree];

		if (!levelByType.TryAdd(nodeType, levelAdjustment))
		{
			if (set)
			{
				levelByType[nodeType] += levelAdjustment;
			}
			else
			{
				levelByType[nodeType] = levelAdjustment;
			}
		}

		if (Main.netMode == NetmodeID.MultiplayerClient && sync)
		{
			SkillPassiveValueHandler.Send((byte)Player.whoAmI, tree.GetType().FullName, nodeType.FullName, (byte)levelByType[nodeType]);
		}
	}

	/// <summary>
	/// Gets the strength of the given passive on the given tree.
	/// </summary>
	/// <typeparam name="TTree">The tree to reference.</typeparam>
	/// <typeparam name="TPassive">The passive to get the level of.</typeparam>
	/// <returns>The cumulative levels of a given passive on a tree.</returns>
	internal int GetPassiveStrength<TTree, TPassive>() where TTree : SkillTree where TPassive : SkillPassive
	{
		if (!TotalLevelByTypeByTree.TryGetValue(ModContent.GetInstance<TTree>(), out Dictionary<Type, int> lookup) || !lookup.TryGetValue(typeof(TPassive), out int level))
		{
			return 0;
		}

		return level;
	}

	/// <summary>
	/// Checks if the player has a given passive on a tree at all.
	/// </summary>
	/// <typeparam name="TTree">The tree to reference.</typeparam>
	/// <typeparam name="TPassive">The passive to check for.</typeparam>
	/// <returns>If the player has any one or more of the passive enabled.</returns>
	internal bool HasPassive<TTree, TPassive>() where TTree : SkillTree where TPassive : SkillPassive
	{
		return GetPassiveStrength<TTree, TPassive>() > 0;
	}

	/// <summary>
	/// Checks if the given skill's specialization is <typeparamref name="TSpecialization"/>.
	/// </summary>
	/// <typeparam name="TSkill">The skill to reference.</typeparam>
	/// <typeparam name="TSpecialization">The specialization to match for.</typeparam>
	/// <returns>If the specialization for <typeparamref name="TSkill"/> is <typeparamref name="TSpecialization"/>.</returns>
	internal bool HasSpecialization<TSkill, TSpecialization>() where TSkill : Skill where TSpecialization : SkillSpecial
	{
		if (!SpecializationsBySkill.TryGetValue(typeof(TSkill), out SkillSpecial spec))
		{
			return false;
		}

		return spec is TSpecialization;
	}

	/// <summary>
	/// Gets the specialization currently on <typeparamref name="TSkill"/>, if any. Returns null if there isn't one.
	/// </summary>
	/// <typeparam name="TSkill">The skill to get the specialization of.</typeparam>
	/// <returns>The skill's specialization, if any.</returns>
	internal SkillSpecial GetSpecialization<TSkill>() where TSkill : Skill
	{
		if (SpecializationsBySkill.TryGetValue(typeof(TSkill), out SkillSpecial spec))
		{
			return spec;
		}

		return null;
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("count", TotalLevelByTypeByTree.Count);
		int count = 0;

		foreach (SkillTree tree in TotalLevelByTypeByTree.Keys)
		{
			TagCompound treeTag = [];
			SaveIndividualTree(tree, treeTag);
			tag.Add("tree" + count++, treeTag);
		}

		tag.Add("specCount", SpecializationsBySkill.Count);
		count = 0;

		foreach (KeyValuePair<Type, SkillSpecial> skill in SpecializationsBySkill)
		{
			tag.Add("specName" + count, skill.Key.FullName);
			tag.Add("specValue" + count, skill.Value.GetType().FullName);
			count++;
		}
	}

	private void SaveIndividualTree(SkillTree tree, TagCompound treeTag)
	{
		Dictionary<Type, int> passives = TotalLevelByTypeByTree[tree];
		treeTag.Add("name", tree.GetType().FullName);
		treeTag.Add("count", passives.Count);
		int count = 0;

		foreach (KeyValuePair<Type, int> pair in passives)
		{
			TagCompound pairTag = [];
			pairTag.Add("name", pair.Key.FullName);
			pairTag.Add("str", pair.Value);
			
			treeTag.Add("pair" + count, pairTag);
			count++;
		}
	}

	public override void LoadData(TagCompound tag)
	{
		TotalLevelByTypeByTree.Clear();
		SpecializationsBySkill.Clear();
		int count = tag.GetInt("count");

		for (int i = 0; i < count; ++i)
		{
			TagCompound treeTag = tag.GetCompound("tree" + i);
			LoadIndividualTree(treeTag);
		}

		count = tag.GetInt("specCount");

		for (int i = 0; i < count; ++i)
		{
			AddSkillSpecBasedOnTypeNames(tag.GetString("specValue" + i), tag.GetString("specName" + i));
		}
	}

	/// <summary>
	///	Gets the info for and calls <see cref="SetSpecializationForSkill(Type, SkillSpecial, bool)"/> 
	///	based on the <see cref="Type.FullName"/> of the specialization and skill.
	/// </summary>
	/// <param name="specName"><see cref="Type.FullName"/> of the specialization.</param>
	/// <param name="skillName"><see cref="Type.FullName"/> of the skill.</param>
	/// <param name="sync">Whether this should call sync on <see cref="SetSpecializationForSkill(Type, SkillSpecial, bool)"/>.</param>
	internal void AddSkillSpecBasedOnTypeNames(string specName, string skillName, bool sync = true)
	{
		Assembly assembly = GetType().Assembly;
		Type specType = assembly.GetType(specName);
		Type skillType = assembly.GetType(skillName);
		SkillTree tree = SkillTree.TypeToSkillTree[skillType];
		var spec = Activator.CreateInstance(specType, tree) as SkillSpecial;
		SetSpecializationForSkill(skillType, spec, sync);
	}

	private void LoadIndividualTree(TagCompound treeTag)
	{
		string name = treeTag.GetString("name");
		int count = treeTag.GetInt("count");
		Dictionary<Type, int> passives = [];

		for (int i = 0; i < count; ++i)
		{
			TagCompound pairTag = treeTag.GetCompound("pair" + i);
			string passiveName = pairTag.GetString("name");
			int str = pairTag.GetInt("str");
			passives.Add(typeof(PoTMod).Assembly.GetType(passiveName), str);
		}

		Type treeType = typeof(PoTMod).Assembly.GetType(name);
		var tree = Activator.CreateInstance(treeType) as SkillTree;
		TotalLevelByTypeByTree.Add(SkillTree.TypeToSkillTree[tree.ParentSkill], passives);
	}
}
