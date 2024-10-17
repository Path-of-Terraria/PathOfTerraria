using System.Collections.Generic;

namespace PathOfTerraria.Common.Waypoints;

/// <summary>
///     Manages the registration of <see cref="ModWaypoint" /> instances.
/// </summary>
public static class ModWaypointLoader
{
	/// <summary>
	///     The list of registered <see cref="ModWaypoint" /> instances.
	/// </summary>
	public static readonly List<ModWaypoint> Waypoints = [];

	/// <summary>
	///     The total amount of registered <see cref="ModWaypoint" /> instances. Shorthand for <c>ModWaypointLoader.Waypoints.Count</c>.
	/// </summary>
	public static int WaypointCount => Waypoints.Count;

	/// <summary>
	///     Registers a <see cref="ModWaypoint" /> instance.
	/// </summary>
	/// <param name="waypoint">The value of the <see cref="ModWaypoint" /> instance.</param>
	/// <typeparam name="T">The type of the <see cref="ModWaypoint" /> instance.</typeparam>
	public static void Register<T>(T waypoint) where T : ModWaypoint
	{
		Waypoints.Add(waypoint);
	}
}