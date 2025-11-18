
namespace PathOfTerraria.Common.UI.GrimoireSelection;

internal class GrimoireShiftClickPlayer : ModPlayer
{
	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		if (GrimoireSelectionUIState.DrawingSlots)
		{

			return true;
		}

		return false;
	}
}
