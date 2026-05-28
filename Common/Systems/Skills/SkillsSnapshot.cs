using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.Skills;

public sealed class SkillsSnapshot
{
	public required List<EquippedSkillSnapshot> Skills { get; set; } = [];
}

public sealed class EquippedSkillSnapshot
{
	public int SlotIndex { get; set; }
	public required string SourceMod { get; set; }
	public required string InternalName { get; set; }
	public required string DisplayName { get; set; }
	public int Level { get; set; }
	public int MaxLevel { get; set; }
	public int Points { get; set; }
	public string SpecializationInternalName { get; set; }
	public string SpecializationDisplayName { get; set; }
	public required List<SkillTreeNodeSnapshot> Nodes { get; set; } = [];
	public required List<SkillTreeEdgeSnapshot> Edges { get; set; } = [];
}

public sealed class SkillTreeNodeSnapshot
{
	public required string InternalIdentifier { get; set; }
	public required string DisplayName { get; set; }
	public string DisplayTooltip { get; set; }
	public float PosX { get; set; }
	public float PosY { get; set; }
	public int Level { get; set; }
	public int MaxLevel { get; set; }
	public bool IsSpecialization { get; set; }
	public bool IsAnchor { get; set; }
	public bool IsHidden { get; set; }
}

public sealed class SkillTreeEdgeSnapshot
{
	public required string FromInternalIdentifier { get; set; }
	public required string ToInternalIdentifier { get; set; }
}
