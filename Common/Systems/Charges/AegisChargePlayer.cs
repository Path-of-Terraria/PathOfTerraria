namespace PathOfTerraria.Common.Systems.Charges;

public class AegisChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Content.Buffs.AegisChargeBuff>();

	public int AegisChargeHealthBonus = 20;
	public int AegisDefenseBonus = 5;
	public float AegisPercentHealthBonus = 0f;
		
	public AegisChargePlayer()
	{
		ChargeColor = Color.Red;
		ChargeName = "Aegis";
		ChargeGainChance = 0;
	}
	
	protected override void InternalResetEffects()
	{
		AegisPercentHealthBonus = 0f;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply flat health bonus
		modifier.MaximumLife.Flat += AegisChargeHealthBonus * Charges;
    
		// Apply damage reduction multiplier
		modifier.Defense.Base += AegisDefenseBonus * Charges;
		
		// Apply percent health bonus (from the passive, if enabled)
		if (AegisPercentHealthBonus > 0f)
		{
			float percentBonus = AegisPercentHealthBonus * Charges;
			modifier.MaximumLife *= (1f + percentBonus);
		}
	}

	public override void PostUpdateEquips()
	{
		if (HasAnyCharges)
		{
			EntityModifier modifier = new EntityModifier();
			ApplyChargeModifiers(modifier);
			modifier.ApplyTo(Player);
		}
	}

}