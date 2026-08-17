using SubworldLibraryCommunityFork;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// Base class for persistent town subworlds. Towns are destinations rather than instanced runs, so
/// leaving one must not create a remembered position that bypasses its normal arrival point later.
/// </summary>
internal abstract class TownSubworld : MappingWorld
{
	public sealed override SubworldReturnPositionMode ReturnPositionMode => SubworldReturnPositionMode.Disabled;
}
