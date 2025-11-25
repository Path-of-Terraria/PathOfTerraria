namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class OldSummonSlotPlayer : ModPlayer
{
	public float OldSlots { get; private set; }

	public override void ResetEffects()
	{
		OldSlots = Player.slotsMinions;
	}
}
