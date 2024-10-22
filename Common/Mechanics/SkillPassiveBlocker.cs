using System.Collections.Generic;

namespace PathOfTerraria.Common.Mechanics;

/// <summary>
/// Allows additional checks to block skill passives from being allocated.<br/>
/// These are persistent and always active.
/// </summary>
public abstract class SkillPassiveBlocker : ILoadable
{
	public static readonly Dictionary<string, SkillPassiveBlocker> LoadedBlockers = [];

	public string FullName => Mod.Name + "/" + GetType().Name;
	
	internal Mod Mod = null;

	public void Load(Mod mod)
	{
		Mod = mod;
		LoadedBlockers.Add(FullName, this);
	}

	public void Unload()
	{
		LoadedBlockers.Remove(FullName);
	}

	public abstract bool BlockAllocation(SkillPassive passive);
}
