using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Systems.Charges;

public class HasteChargePlayer : ModChargePlayer
{
	protected override int BuffType => ModContent.BuffType<Content.Buffs.HasteChargeBuff>();

	public float HasteChargeBonus = 0.05f;
	public float HasteProjectileSpeedBonus = 0f;
		
	public HasteChargePlayer()
	{
		ChargeColor = Color.Green;
		ChargeName = "Haste";
		ChargeGainChance = 0;
	}
	
	protected override void InternalResetEffects()
	{
		HasteProjectileSpeedBonus = 0f;
	}
    
	protected override void ApplyChargeModifiers(EntityModifier modifier)
	{
		// Apply attack speed and movement speed bonuses
		float multiplier = 1f + (HasteChargeBonus * Charges);
		modifier.AttackSpeed *= multiplier;
		modifier.MovementSpeed *= multiplier;
		
		// Apply percentage speed bonus per charge
		if (HasteProjectileSpeedBonus > 0f)
		{
			UniversalBuffingPlayer universalPlayer = Main.LocalPlayer.GetModPlayer<UniversalBuffingPlayer>();
			float percentBonus = HasteProjectileSpeedBonus * Charges;
			universalPlayer.UniversalModifier.ProjectileSpeed *= (1f + percentBonus);
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