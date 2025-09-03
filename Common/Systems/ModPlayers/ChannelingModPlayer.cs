namespace PathOfTerraria.Common.Systems.ModPlayers;

public class ChannelingModPlayer : ModPlayer
{
	public float ChannelingDamageBonus;

	public override void ResetEffects()
	{
		ChannelingDamageBonus = 0f;
	}
}
