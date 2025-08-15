namespace PathOfTerraria.Common.Systems.Charges;

public class FocusChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Common.Buffs.FocusChargeBuff>();

	public float FocusChargeBonus = 0.05f;
		
	public FocusChargePlayer()
	{
		ChargeColor = Color.Blue;
		ChargeName = "Focus";
		ChargeGainChance = 0;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply flat critical strike chance and critical strike multiplier
		modifier.CriticalChance.Flat += FocusChargeBonus * Charges * 100; // Back to * 100
		modifier.CriticalDamage.Base += modifier.CriticalChance.Base * FocusChargeBonus;
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