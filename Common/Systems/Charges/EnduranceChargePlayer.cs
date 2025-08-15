namespace PathOfTerraria.Common.Systems.Charges;

public class EnduranceChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Common.Buffs.EnduranceChargeBuff>();

	public int EnduranceChargeHealthBonus = 20;
	public float EnduranceDamageReductionBonus = 0.05f;
		
	public EnduranceChargePlayer()
	{
		ChargeColor = Color.Red;
		ChargeName = "Endurance";
		ChargeGainChance = 0;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply flat health bonus
		modifier.MaximumLife.Flat += EnduranceChargeHealthBonus * Charges;
    
		// Apply damage reduction multiplier
		float reductionMultiplier = 1f + (EnduranceDamageReductionBonus * Charges);
		modifier.DamageReduction *= reductionMultiplier;
	}


	public override void PostUpdateEquips()
	{
		if (HasAnyCharges())
		{
			EntityModifier modifier = new EntityModifier();
			ApplyChargeModifiers(modifier);
			modifier.ApplyTo(Player);
		}
	}

}