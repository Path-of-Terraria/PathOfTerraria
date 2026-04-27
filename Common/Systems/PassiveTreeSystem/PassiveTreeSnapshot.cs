using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.PassiveTreeSystem;

public sealed class PassiveTreeSnapshot
{
	public int Points { get; set; }
	public int ExtraPoints { get; set; }
	public required List<PassiveTreeNodeSnapshot> AllocatedNodes { get; set; } = [];
}

public sealed class PassiveTreeNodeSnapshot
{
	public int ReferenceId { get; set; }
	public required string InternalIdentifier { get; set; }
	public int Level { get; set; }
}
