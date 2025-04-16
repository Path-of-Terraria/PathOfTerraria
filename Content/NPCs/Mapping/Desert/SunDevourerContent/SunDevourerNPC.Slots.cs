namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourerContent;

public sealed partial class SunDevourerNPC// : ModNPC
{
	/// <summary>
	///		Gets or sets the mode of the NPC. Shorthand for <c>NPC.ai[0]</c>.
	/// </summary>
	public ref float Mode => ref NPC.ai[0];
	
	/// <summary>
	///		Gets or sets the state of the NPC. Shorthand for <c>NPC.ai[1]</c>.
	/// </summary>
	public ref float State => ref NPC.ai[1];
	
	/// <summary>
	///		Gets or sets the timer of the NPC. Shorthand for <c>NPC.ai[2]</c>.
	/// </summary>
	public ref float Timer => ref NPC.ai[2];

	/// <summary>
	///		Gets or sets the counter of the NPC. Shorthand for <c>NPC.ai[3]</c>.
	/// </summary>
	public ref float Counter => ref NPC.ai[3];
	
	/// <summary>
	///		Gets or sets the previous state of the NPC. Shorthand for <c>NPC.localAI[0]</c>.
	/// </summary>
	public ref float Previous => ref NPC.localAI[0];
}