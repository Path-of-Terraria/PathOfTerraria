using PathOfTerraria.Core.Subworlds.Passes;
using PathOfTerraria.Core.Systems;
using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Core.Subworlds;

/// <summary>
/// This is used to stop bosses from doing special death effects (like King Slime spawning the town slime, WoF spawning hardmode) automatically.<br/>
/// That effect is cached in <see cref="BossTracker.CachedBossesDowned"/>.
/// </summary>
public abstract class BossDomainSubworld : MappingWorld
{
	public override bool ShouldSave => false;
	public override bool NoPlayerSaving => false;

	// We are going to first set the world to be completely flat so we can build on top of that
	public override List<GenPass> Tasks => [new FlatWorldPass()];
}