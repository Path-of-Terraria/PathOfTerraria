using Terraria.ModLoader.IO;
using Terraria.ID;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;

namespace PathOfTerraria.Common.Systems.Runebound;

internal sealed class RuneboundPlayer : ModPlayer
{
	public bool TutorialEncounterRequested;
	public bool TutorialEncounterCompleted;
	public bool HasCraftedRunestone;
	public bool LearnedMechanic;
	public int EncountersCompleted;

	public void RequestTutorialEncounter()
	{
		TutorialEncounterRequested = true;

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			RuneboundTutorialRequestHandler.Send();
		}
	}

	public void SyncState()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			RuneboundStateHandler.Send(this);
		}
	}

	public override void OnEnterWorld()
	{
		SyncState();
		if (Main.netMode == NetmodeID.MultiplayerClient && TutorialEncounterRequested && !TutorialEncounterCompleted)
		{
			RuneboundTutorialRequestHandler.Send();
		}
	}

	public override void SaveData(TagCompound tag)
	{
		tag["tutorialRequested"] = TutorialEncounterRequested;
		tag["tutorialCompleted"] = TutorialEncounterCompleted;
		tag["craftedRunestone"] = HasCraftedRunestone;
		tag["learnedMechanic"] = LearnedMechanic;
		tag["encountersCompleted"] = EncountersCompleted;
	}

	public override void LoadData(TagCompound tag)
	{
		TutorialEncounterRequested = tag.GetBool("tutorialRequested");
		TutorialEncounterCompleted = tag.GetBool("tutorialCompleted");
		HasCraftedRunestone = tag.GetBool("craftedRunestone");
		LearnedMechanic = tag.GetBool("learnedMechanic");
		EncountersCompleted = tag.GetInt("encountersCompleted");
	}
}
