using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using SubworldLibrary;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is used to stop bosses from doing special death effects (like King Slime spawning the town slime, WoF spawning hardmode) automatically,<br/>
/// using a nicer loading screen dialogue, and setting <see cref="WorldGenerator.CurrentGenerationProgress"/> as it's not set by default.<br/>
/// The death effect system is in <see cref="BossTracker.CachedBossesDowned"/>.
/// </summary>
public abstract class BossDomainSubworld : MappingWorld
{
	public override bool ShouldSave => false;
	public override bool NoPlayerSaving => false;

	/// <summary>
	/// The level of dropped <see cref="Content.Items.Gear.Gear"/> in the domain. 0 will roll default level formula.
	/// </summary>
	public virtual int DropItemLevel => 0;

	/// <summary>
	/// Forces the time to be the given time, and it to be night/day. Defaults to (-1, true), which ignores this.
	/// </summary>
	public virtual (int time, bool isDay) ForceTime => (-1, true);

	// We are going to first set the world to be completely flat so we can build on top of that
	public override List<GenPass> Tasks => [new FlatWorldPass()];

	public override void OnEnter()
	{
		base.OnEnter();

		SubworldSystem.noReturn = true;
	}
}
