using PathOfTerraria.Common.Mechanics;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Skills;

/// <summary>
/// Used solely to delay loading trees. Has no functionality outside of loading cached data.
/// </summary>
internal class SkillTreePlayer : ModPlayer
{
	public readonly record struct CachedSkillTreeData(SkillTree Instance, Skill Skill, TagCompound Tag);

	private readonly List<CachedSkillTreeData> cachedData = [];

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
	}
}
