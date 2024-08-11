using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Skills.Melee;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Mechanics;

internal class SkillPassiveEdge(SkillPassive start, SkillPassive end)
{
	public readonly SkillPassive Start = start;
	public readonly SkillPassive End = end;

	public bool Contains(SkillPassive p)
	{
		return p == Start || p == End;
	}

	/// <summary>
	/// Assuming that p is either start or end - Contains returned true.
	/// </summary>
	public SkillPassive Other(SkillPassive p)
	{
		return p == Start ? End : Start;
	}
}

public abstract class SkillPassive
{
	public byte Level = 1;
	public Vector2 TreePos;

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
}