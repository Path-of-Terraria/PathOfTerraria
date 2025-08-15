namespace PathOfTerraria.Common.Systems.Charges;

public class PowerChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Common.Buffs.PowerChargeBuff>();

	public float PowerChargeBonus = 0.05f;
		
	public PowerChargePlayer()
	{
		ChargeColor = Color.Blue;
		ChargeName = "Power";
		ChargeGainChance = 0;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply flat critical strike chance and critical strike multiplier
		float multiplier = 1f + (PowerChargeBonus * Charges);
		modifier.CriticalChance.Flat += PowerChargeBonus * Charges * 100; // Back to * 100
		modifier.CriticalDamage.Base += modifier.CriticalChance.Base * PowerChargeBonus;
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