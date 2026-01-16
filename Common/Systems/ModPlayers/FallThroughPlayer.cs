namespace PathOfTerraria.Common.Systems.ModPlayers;

// Copied from a similar implementation in Spirit Reforged
internal class FallThroughPlayer : ModPlayer
{
	/// <summary> Set to true if the player should fall through a platform validated by <see cref="FallThrough"/>. </summary>
	public bool FallThroughPlatform;

	private bool _noReset;

	/// <summary> Should be checked continuously while the player is intersecting with custom platform. See <see cref="FallThrough"/>. </summary>
	/// <returns> Whether the player is falling through. </returns>
	public bool FallThrough()
	{
		_noReset = true;
		return FallThroughPlatform || Player.grapCount > 0;
	}

	public override void ResetEffects()
	{
		if (!_noReset)
		{
			FallThroughPlatform = false;
		}

		_noReset = false;
	}
}
