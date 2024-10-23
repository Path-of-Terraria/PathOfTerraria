using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.NPCs.OverheadDialogue;

/// <summary>
/// Marks that a given NPC is has overhead dialogue, when it happens, and what it is.<br/>
/// <see cref="CurrentDialogue"/> makes sure that each NPC stores their own dialogue,<br/>
/// and <see cref="Name"/> is for getting the name of the current NPC automatically as ModNPC already defines Name.
/// </summary>
internal interface IOverheadDialogueNPC
{
	public OverheadDialogueInstance CurrentDialogue { get; set; }
	public string Name { get; }

	/// <summary>
	/// When to show dialogue. Defaults to a 1/1200 chance per frame.
	/// </summary>
	/// <returns>If dialogue should pop up or not.</returns>
	public bool ShowDialogue()
	{
		return Main.rand.NextBool(1200);
	}

	/// <summary>
	/// What the new dialogue will say. Called immediately after <see cref="ShowDialogue"/>. Defaults to 3 variants of the following conditions:<br/>
	/// <c>Goblins, Rain, Day, Night</c>
	/// </summary>
	/// <returns>New dialogue text.</returns>
	public string GetDialogue()
	{
		string baseNPC = $"Mods.PathOfTerraria.NPCs.{Name}.BubbleDialogue.";

		if (Main.invasionType == InvasionID.GoblinArmy)
		{
			return Language.GetTextValue(baseNPC + "Goblins." + Main.rand.Next(3));
		}

		if (Main.IsItRaining)
		{
			return Language.GetTextValue(baseNPC + "Rain." + Main.rand.Next(3));
		}

		if (Main.dayTime)
		{
			return Language.GetTextValue(baseNPC + "Day." + Main.rand.Next(3));
		}

		return Language.GetTextValue(baseNPC + "Night." + Main.rand.Next(3));
	}
}
