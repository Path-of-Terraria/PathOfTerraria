using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds.Passes;
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

	// We are going to first set the world to be completely flat so we can build on top of that
	public override List<GenPass> Tasks => [new FlatWorldPass()];

	// Sets the time to the middle of the day whenever the subworld loads
	public override void OnLoad()
	{
		//Main.dayTime = true;
		//Main.time = 27000;
	}
}