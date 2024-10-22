namespace PathOfTerraria.Common.NPCs.OverheadDialogue;

internal class OverheadDialogueGlobalNPC : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return entity.ModNPC is IOverheadDialogueNPC;
	}

	public override void PostAI(NPC npc)
	{
		var dialog = npc.ModNPC as IOverheadDialogueNPC;

		if (Main.LocalPlayer.TalkNPC is not null && Main.LocalPlayer.TalkNPC.whoAmI == npc.whoAmI)
		{
			dialog.CurrentDialogue = null;
		}

		dialog.CurrentDialogue?.Update();
	}
}
