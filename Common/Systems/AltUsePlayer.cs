using PathOfTerraria.Common.UI.Utilities;

namespace PathOfTerraria.Common.Systems;

public class AltUsePlayer : ModPlayer
{
	/// <summary>
	/// How long until the cooldown is up.
	/// </summary>
	public int AltFunctionCooldown { get; private set; }

	/// <summary>
	/// Max cooldown, mainly for use in <see cref="AltUseUISystem"/>'s bar UI.
	/// </summary>
	public int MaxAltCooldown { get; private set; }

	/// <summary>
	/// This is how long the alt use will be active for. This is meant for anything that has a duration.
	/// </summary>
	public int AltFunctionActiveTimer { get; private set; }

	public bool AltFunctionActive => AltFunctionActiveTimer > 0;
	public bool AltFunctionAvailable => !OnCooldown && !BlockClickItem.Block;
	public bool OnCooldown => AltFunctionCooldown > 0;

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

	/// <summary>
	/// Sets <see cref="AltFunctionCooldown"/> and <see cref="MaxAltCooldown"/> to <paramref name="altCooldown"/>
	/// and <see cref="AltFunctionActiveTimer"/> to <paramref name="activeTime"/>.
	/// </summary>
	/// <param name="altCooldown"></param>
	/// <param name="activeTime"></param>
	public void SetAltCooldown(int altCooldown, int activeTime = 0) 
	{ 
		AltFunctionCooldown = altCooldown;
		MaxAltCooldown = altCooldown;
		AltFunctionActiveTimer = activeTime;
	}
}