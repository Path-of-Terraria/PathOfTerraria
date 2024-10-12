using System.Runtime.CompilerServices;
using PathOfTerraria.Common.Waypoints.UI;
using Terraria.Localization;

namespace PathOfTerraria.Common.Waypoints;

public abstract class ModWaypoint : ModType, ILocalizedModType
{
	public virtual string LocalizationCategory { get; } = "Waypoints";

	/// <summary>
	///		The translations for the display name of this waypoint.
	/// </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	///		The translations for the description of this waypoint.
	/// </summary>
	public virtual LocalizedText Description => this.GetLocalization(nameof(Description), PrettyPrintName);

	/// <summary>
	///		The qualified path to the icon texture of this waypoint.
	/// </summary>
	public virtual string IconPath => GetType().FullName.Replace('.', '/') + "_Icon";

	/// <summary>
	///		The qualified path to the preview texture of this waypoint.
	/// </summary>
	public virtual string PreviewPath => GetType().FullName.Replace('.', '/') + "_Preview";

	public override void SetStaticDefaults()
	{
		_ = DisplayName;
		_ = Description;
	}

	protected sealed override void Register()
	{
		ModWaypointLoader.Register(this);

		ModTypeLookup<ModWaypoint>.Register(this);
	}

	public abstract void Teleport(Player player);
}