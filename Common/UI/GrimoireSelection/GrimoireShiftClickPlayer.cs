using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ID;

namespace PathOfTerraria.Common.UI.GrimoireSelection;

internal class GrimoireShiftClickPlayer : ModPlayer
{
	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		if (GrimoireSelectionUIState.DrawingSlots)
		{
			GrimoirePlayer storagePlayer = Main.LocalPlayer.GetModPlayer<GrimoirePlayer>();

			Item item = inventory[slot];
			storagePlayer.Storage.Add(item.Clone());
			item.TurnToAir();

			GrimoireSelectionUIState.RefreshStorage();
			return true;
		}

		return false;
	}
}
