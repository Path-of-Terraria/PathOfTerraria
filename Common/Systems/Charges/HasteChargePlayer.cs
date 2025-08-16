namespace PathOfTerraria.Common.Systems.Charges;

public class HasteChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Content.Buffs.HasteChargeBuff>();

	public float HasteChargeBonus = 0.05f;
		
	public HasteChargePlayer()
	{
		ChargeColor = Color.Green;
		ChargeName = "Haste";
		ChargeGainChance = 0;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply attack speed and movement speed bonuses
		float multiplier = 1f + (HasteChargeBonus * Charges);
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