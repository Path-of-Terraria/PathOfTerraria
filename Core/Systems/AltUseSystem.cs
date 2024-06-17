namespace PathOfTerraria.Core.Systems;

public class AltUseSystem : ModPlayer
{
	/// <summary>
	/// This is the time between alt use activations / usage
	/// </summary>
	public int AltFunctionCooldown;
	
	/// <summary>
	/// This is how long the alt use will be active for. This is meant for anything that has a duration.
	/// </summary>
	public int AltFunctionActiveTimer;
	
	public bool AltFunctionActive => AltFunctionActiveTimer > 0;

	/// <summary>
	/// Hits every tick to reduce the cooldown and active timer.
	/// </summary>
	public override void ResetEffects()
	{
		if (AltFunctionCooldown > 0)
		{
			AltFunctionCooldown--;
		}

		if (AltFunctionActiveTimer > 0)
		{
			AltFunctionActiveTimer--;
		}
	}
}