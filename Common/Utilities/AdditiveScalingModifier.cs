using Terraria;
using Terraria.ModLoader;

namespace PathOfTerraria.Common.Utilities;


public static class AdditiveScalingModifier
{
	public static void ApplyAdditiveLikeScalingProjectile(Player player, Projectile projectile,  ref NPC.HitModifiers modifiers, float bonus)
	{
		if (player == null || projectile == null)
			return;

		// --- Get all additives from player based on type of damage of the projectile ---
		float additive = player.GetDamage(DamageClass.Generic).Additive -1;

		if (projectile.DamageType.CountsAsClass(DamageClass.Melee))
			additive += player.GetDamage(DamageClass.Melee).Additive -1;

		if (projectile.DamageType.CountsAsClass(DamageClass.Ranged))
			additive += player.GetDamage(DamageClass.Ranged).Additive -1;

		if (projectile.DamageType.CountsAsClass(DamageClass.Magic))
			additive += player.GetDamage(DamageClass.Magic).Additive -1;

		if (projectile.DamageType.CountsAsClass(DamageClass.Summon))
			additive += player.GetDamage(DamageClass.Summon).Additive -1;
		
		 // --- Calculated the modifier based on existing class bonuses .
		 modifiers.SourceDamage += bonus / (1 + additive) ;
	}
	
	public static void ApplyAdditiveLikeScalingItem(Player player, Item item,  ref NPC.HitModifiers modifiers, float bonus)
	{
		if (player == null || item == null)
			return;

		// --- Get all additives from player based on type of damage of the hit ---
		float additive = player.GetDamage(DamageClass.Generic).Additive -1;

		if (item.DamageType.CountsAsClass(DamageClass.Melee))
			additive += player.GetDamage(DamageClass.Melee).Additive -1;

		if (item.DamageType.CountsAsClass(DamageClass.Ranged))
			additive += player.GetDamage(DamageClass.Ranged).Additive -1;

		if (item.DamageType.CountsAsClass(DamageClass.Magic))
			additive += player.GetDamage(DamageClass.Magic).Additive -1;

		if (item.DamageType.CountsAsClass(DamageClass.Summon))
			additive += player.GetDamage(DamageClass.Summon).Additive -1;
		
		// --- Calculated the modifier based on existing class bonuses .
		modifiers.SourceDamage += bonus / (1 + additive) ;
	}
}