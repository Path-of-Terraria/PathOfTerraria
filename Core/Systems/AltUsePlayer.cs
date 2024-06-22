namespace PathOfTerraria.Core.Systems;

public class AltUsePlayer : ModPlayer
{
	public int AltFunctionCooldown { get; private set; }
	public int MaxAltCooldown { get; private set; }

	public override void ResetEffects()
	{
		if (AltFunctionCooldown > 0)
		{
			AltFunctionCooldown--;
		}
	}

	public void SetAltCooldown(int altCooldown) 
	{ 
		AltFunctionCooldown = altCooldown;
		MaxAltCooldown = altCooldown;
	}
}