namespace PathOfTerraria.Common.Systems.Charges;

public class FrenzyChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Common.Buffs.FrenzyChargeBuff>();

	public float FrenzyChargeBonus = 0.05f;
		
	public FrenzyChargePlayer()
	{
		ChargeColor = Color.Green;
		ChargeName = "Frenzy";
		ChargeGainChance = 0;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply attack speed and movement speed bonuses
		float multiplier = 1f + (FrenzyChargeBonus * Charges);
		modifier.AttackSpeed *= multiplier;
		modifier.MovementSpeed *= multiplier;
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