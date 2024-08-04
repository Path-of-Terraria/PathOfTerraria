namespace PathOfTerraria.Common.NPCs.Components;

public abstract class NPCComponent : GlobalNPC
{
	public sealed override bool InstancePerEntity { get; } = true;
	
	/// <summary>
	///		Whether the component is enabled or not.
	/// </summary>
	public bool Enabled { get; set; }
}