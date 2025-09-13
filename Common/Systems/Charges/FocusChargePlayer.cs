namespace PathOfTerraria.Common.Systems.Charges;

public class FocusChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Content.Buffs.FocusChargeBuff>();

	public float FocusChargeBonus = 0.05f;
	public float FocusChargeDamageBonus = 0f;
		
	public FocusChargePlayer()
	{
		ChargeColor = Color.Blue;
		ChargeName = "Focus";
		ChargeGainChance = 0;
	}
	
	public override void ResetEffects()
	{
		FocusChargeDamageBonus = 0f;
		ChargeGainChance = 0;
		MaxCharges = 3;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply flat critical strike chance and critical strike multiplier
		modifier.CriticalChance.Flat += FocusChargeBonus * Charges * 100; // Back to * 100
		modifier.CriticalDamage.Base += modifier.CriticalChance.Base * FocusChargeBonus;
		
		// Apply percentage damage bonus per charge
		if (FocusChargeDamageBonus > 0f)
		{
			float percentBonus = FocusChargeDamageBonus * Charges;
			modifier.Damage *= (1f + percentBonus);
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