using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.DisableBuilding;
using SubworldLibrary;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is the base class for all mapping worlds. It sets the width and height of the world to 1000x1000 and disables world saving.<br/>
/// Additionally, it also makes <see cref="Systems.DisableBuilding.StopBuildingPlayer"/> disable world modification.
/// </summary>
public abstract class MappingWorld : Subworld
{
	public override int Width => 1000;
	public override int Height => 1000;

	public override bool ShouldSave => false;
	public override bool NoPlayerSaving => false;
	
	/// <summary>
	/// These tiles are allowed to be mined by the player using a pickaxe.
	/// </summary>
	public virtual int[] WhitelistedMiningTiles => [];

	/// <summary>
	/// These tiles are allowed to be cut by the player with melee or projectiles.
	/// </summary>
	public virtual int[] WhitelistedCutTiles => [];

	/// <summary>
	/// These tiles are allowed to be placed by the player. These should also be in <see cref="WhitelistedMiningTiles"/>.
	/// </summary>
	public virtual int[] WhitelistedPlaceableTiles => [];

	// We are going to first set the world to be completely flat so we can build on top of that
	public override List<GenPass> Tasks => [new FlatWorldPass()];

	/// <summary>
	/// Forces the time to be the given time, and it to be night/day. Defaults to (-1, true), which ignores this.
	/// </summary>
	public virtual (int time, bool isDay) ForceTime => (-1, true);

	public List<MapAffix> Affixes = null;

	public int Level = 0;
	
	internal virtual void ModifyDefaultWhitelist(HashSet<int> results, BuildingWhitelist.WhitelistUse use)
	{
	}
}