using PathOfTerraria.Common.Systems;
using Terraria.Localization;

namespace PathOfTerraria.Common.Waypoints;

public abstract class ModWaypoint : ModType, ILocalizedModType
{
	/// <summary>
	///     The translations for the display name of this waypoint.
	/// </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	public abstract string LocationEnum { get; }

	/// <summary>
	///     The qualified path to the icon texture of this waypoint.
	/// </summary>
	public virtual string IconPath => (GetType().Namespace + "." + Name).Replace('.', '/').Replace("Common", "Assets") + "_Icon";

	/// <summary>
	///     The qualified path to the preview texture of this waypoint.
	/// </summary>
	public virtual string PreviewPath => (GetType().Namespace + "." + Name).Replace('.', '/').Replace("Common", "Assets") + "_Preview";

	public virtual string LocalizationCategory { get; } = "Waypoints";

	protected sealed override void Register()
	{
		ModWaypointLoader.Register(this);

		ModTypeLookup<ModWaypoint>.Register(this);
	}

	public abstract void Teleport(Player player);

	/// <summary>
	/// Used to check if the player can go to this waypoint. For example, stopping the player from "going to" the area they're currently in.
	/// </summary>
	/// <returns></returns>
	public abstract bool CanGoto();
}