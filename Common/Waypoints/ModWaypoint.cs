using System.Runtime.CompilerServices;
using PathOfTerraria.Common.Waypoints.UI;
using Terraria.Localization;

// TODO: Implement info attributes for the UI's info tab.
namespace PathOfTerraria.Common.Waypoints;

/// <summary>
///		Provides a base implementation for waypoints.
/// </summary>
/// <remarks>
///		Implementations are automatically registered and included in <see cref="UIWaypointBrowser"/>.
/// </remarks>
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
	///		The qualified path to the waypoint's icon texture.
	/// </summary>
	public virtual string IconPath => GetType().FullName.Replace('.', '/') + "_Icon";
	
	/// <summary>
	///		The qualified path to the waypoint's preview texture.
	/// </summary>
	public virtual string PreviewPath => GetType().FullName.Replace('.', '/') + "_Preview";
	
	protected sealed override void Register()
	{
		ModWaypointLoader.Register(this);
		
		ModTypeLookup<ModWaypoint>.Register(this);
	}

	public abstract void Teleport(Player player);
}