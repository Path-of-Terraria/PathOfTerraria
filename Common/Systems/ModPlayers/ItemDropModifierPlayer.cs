namespace PathOfTerraria.Common.Systems.ModPlayers;

public class ItemDropModifierPlayer : ModPlayer
{
	/// <summary>
	/// Defaults to 0.
	/// 1 Magic find would mean 100% multiplier to quality of items from mobs
	/// -- currently only magic and rare and probably unique mobs will benifit from this,
	/// -- as normal mobs will have 0 item quality increase.
	/// </summary>
	public float MagicFind = 0f;

	/// <summary>
	/// Multiplier on how likely unique items are to drop.<br/>
	/// Defaults to 0.
	/// </summary>
	public float UniqueFindMultiplier = 1f;

	/// <summary>
	/// Multiplier on how many items are dropped.<br/>
	/// Defaults to 1.
	/// </summary>
	public float ItemDropRateMultiplier = 1f;

	public override void ResetEffects()
	{
		MagicFind = 0;
		UniqueFindMultiplier = 1f;
		ItemDropRateMultiplier = 1f;
	}
}
