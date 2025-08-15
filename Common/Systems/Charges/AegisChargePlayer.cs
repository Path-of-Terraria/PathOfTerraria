namespace PathOfTerraria.Common.Systems.Charges;

public class AegisChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Common.Buffs.AegisChargeBuff>();

	public int AegisChargeHealthBonus = 20;
	public int AegisDefenseBonus = 5;
		
	public AegisChargePlayer()
	{
		ChargeColor = Color.Red;
		ChargeName = "Aegis";
		ChargeGainChance = 0;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply flat health bonus
		modifier.MaximumLife.Flat += AegisChargeHealthBonus * Charges;
    
		// Apply damage reduction multiplier
		modifier.Defense.Base += AegisDefenseBonus * Charges;
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