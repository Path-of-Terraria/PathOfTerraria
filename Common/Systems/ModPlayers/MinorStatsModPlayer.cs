namespace PathOfTerraria.Common.Systems.ModPlayers;
public class MinorStatsModPlayer : ModPlayer
{
	/// <summary>
	/// Defaults to 0.
	/// 1 Magic find would mean 100% multiplier to quality of items from mobs
	/// -- currently only magic and rare and probablt unique mobs will benifit from this,
	/// -- as normal mobs will have 0 item quality increase (well, maby maps will add more...)
	/// </summary>
	public float MagicFind = 0f;

	public override void ResetEffects()
	{
		MagicFind *= 0;
	}
}
