using PathOfTerraria.Content.Socketables;

namespace PathOfTerraria.Core.Items;

public sealed class GearInstanceData : GlobalItem
{
	public override bool InstancePerEntity => true;

	protected override bool CloneNewInstances => true;

	/// <summary>
	///		The sockets.
	/// </summary>
	public Socketable[] Sockets { get; set; } = [];

	/// <summary>
	///		The index of the currently-selected socket.
	/// </summary>
	public int SelectedSocket { get; set; }
}

partial class GearGlobalItem;