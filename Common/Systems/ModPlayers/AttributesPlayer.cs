namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class AttributesPlayer : ModPlayer
{
	public int Strength { get; private set; }
	public int Dexterity { get; private set; }
	public int Intelligence { get; private set; }

	public override void ResetEffects()
	{
		Player.statLifeMax += (int)(Strength / 2f);
	}
}
