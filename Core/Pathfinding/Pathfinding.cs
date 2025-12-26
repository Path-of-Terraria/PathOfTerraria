using System.Diagnostics.CodeAnalysis;
using Wayfarer.API;
using Wayfarer.Data;

#nullable enable

namespace PathOfTerraria.Core.Pathfinding;

// Allows safer usage of Wayfarer from Terraria entities.
internal sealed class PathHandle(WayfarerHandle inner) : IDisposable
{
	public WayfarerHandle WfHandle = inner;

	~PathHandle()
	{
		Dispose();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);

		if (WfHandle != WayfarerHandle.Invalid)
		{
			WfHandle.Dispose();
		}
	}
}

internal sealed class Pathfinding : ModSystem
{
	public override void Unload()
	{
		WayfarerAPI.Shutdown();
	}

	public static bool Attempt(NavMeshParameters navMeshParams, NavigatorParameters navigatorParams, [NotNullWhen(true)] out PathHandle? result)
	{
		if (WayfarerAPI.TryCreatePathfindingInstance(navMeshParams, navigatorParams, out WayfarerHandle handle))
		{
			result = new(handle);
			return true;
		}

		result = default;
		return false;
	}
}
