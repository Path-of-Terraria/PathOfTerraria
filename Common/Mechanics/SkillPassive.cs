using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillPassive
{
	public abstract Skill Skill { get; }
	public virtual List<SkillPassive> Connections { get; set; }
	public abstract int ReferenceId { get; }
	public int Level;
	public abstract Vector2 TreePos { get; }

	public abstract int MaxLevel { get; }

	public virtual string Name => GetType().Name;
	public virtual string Texture => $"{PoTMod.ModName}/Assets/SkillPassives/" + GetType().Name;

	public virtual LocalizedText DisplayName => Language.GetText("Mods.PathOfTerraria.SkillPassive." + Name + ".Name");
	public virtual LocalizedText Description => Language.GetText("Mods.PathOfTerraria.SkillPassive." + Name + ".Description");

	public abstract void LevelTo(byte level);

	protected void IncreaseLevel()
	{
		LevelTo((byte)(Level + 1));
	}

	public virtual void LoadData(TagCompound tag)
	{
		Level = tag.GetByte(nameof(Level));
	}

	public virtual void SaveData(TagCompound tag)
	{
		tag.Add(nameof(Level), Level);
	}

	public bool HasAllocated()
	{
		return Level > 0;
	}
	
	/// <summary>
	/// If this passive is able to be allocated or not
	/// </summary>
	/// <returns></returns>
	public bool CanAllocate()
	{
		return
			Level < MaxLevel &&
			Skill.Edges.Any(e => e.Contains(this) && e.Other(this).Level > 0);
	}
}