using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Systems;

public class EntityModifierSegment
{
	public virtual Dictionary<string, StatModifier> Modifiers => null;
}

public partial class EntityModifier : EntityModifierSegment
{
	private static readonly EntityModifier _default = new();

	public StatModifier MaximumLife = new();
	public StatModifier LifeRegen = new();
	public StatModifier MaximumMana = new();
	public StatModifier ManaRegen = new();
	public StatModifier Defense = new();
	public StatModifier DamageReduction = new();
	public StatModifier MovementSpeed = new();
	public StatModifier ProjectileSpeed = new();
	public StatModifier ProjectileCount = new(); // would be neat
	public StatModifier Damage = new();
	public StatModifier Attackspeed = new();
	public StatModifier ArmorPenetration = new();
	public StatModifier Knockback = new();
	public StatModifier CriticalChance = new();
	public StatModifier CriticalDamage = new();
	public StatModifier CriticalMultiplier = new();
	public OnHitDeBuffer Buffer = new();

	// MinorStatsModPlayer:
	public StatModifier MagicFind = new();

	// PotionSystem:
	public StatModifier MaxHealthPotions = new();
	public StatModifier PotionHealPower = new();

	[ReverseTooltip] 
	public StatModifier PotionHealDelay = new();

	public StatModifier MaxManaPotions = new();
	public StatModifier PotionManaPower = new();

	[ReverseTooltip] 
	public StatModifier PotionManaDelay = new();

	// BuffModifierPlayer:
	public StatModifier DebuffResistance = new();
	public StatModifier BuffBonus = new();

	// ReflectedDamagePlayer:
	public StatModifier ReflectedDamageModifier = new();

	public void ApplyTo(NPC npc)
	{
		npc.life = (int)MaximumLife.ApplyTo(npc.life);
		npc.lifeRegen = (int)LifeRegen.ApplyTo(npc.lifeRegen);
		npc.defense =
			(int)Defense.ApplyTo(npc.defense); // npcs have no such thing as damage reduction, idk how we apply that.
		//	Can only think of a global npc.
		npc.damage = (int)Damage.ApplyTo(npc.damage);

		// there are many things that would need a global npc to be applied.
	}

	public void ApplyTo(Projectile proj)
	{
		proj.damage = (int)Damage.ApplyTo(proj.originalDamage);
	}

	public void ApplyTo(Player player)
	{
		player.statLifeMax2 = (int)MaximumLife.ApplyTo(player.statLifeMax2);

		player.lifeRegen = (int)LifeRegen.ApplyTo(player.lifeRegen); // dont know if this is the right value
		player.statManaMax2 = (int)MaximumMana.ApplyTo(player.statManaMax2);
		// player.statManaMax - for when the overlay gets implemented

		player.manaRegen = (int)ManaRegen.ApplyTo(player.manaRegen); // dont know if this is the right value
		player.statDefense += (int)Defense.ApplyTo((int)player.statDefense) - player.statDefense;

		player.endurance *= (int)DamageReduction.ApplyTo(player.endurance); // i think this is right..?
		player.moveSpeed = MovementSpeed.ApplyTo(player.moveSpeed);
		player.GetDamage(DamageClass.Generic) = player.GetDamage(DamageClass.Generic).CombineWith(Damage);
		player.GetAttackSpeed(DamageClass.Generic) = Attackspeed.ApplyTo(player.GetAttackSpeed(DamageClass.Generic));

		player.GetKnockback(DamageClass.Generic) = player.GetKnockback(DamageClass.Generic).CombineWith(Knockback);
		player.GetArmorPenetration(DamageClass.Generic) =
			Knockback.ApplyTo(player.GetArmorPenetration(DamageClass.Generic));

		MinorStatsModPlayer msmp = player.GetModPlayer<MinorStatsModPlayer>();
		msmp.MagicFind = MagicFind.ApplyTo(msmp.MagicFind);

		PotionSystem ps = player.GetModPlayer<PotionSystem>();
		ps.MaxHealing = (int)MaxHealthPotions.ApplyTo(ps.MaxHealing);
		ps.HealPower = (int)PotionHealPower.ApplyTo(ps.HealPower);
		ps.HealDelay = (int)PotionHealDelay.ApplyTo(ps.HealDelay);

		ps.MaxMana = (int)MaxHealthPotions.ApplyTo(ps.MaxMana);
		ps.ManaPower = (int)PotionManaPower.ApplyTo(ps.ManaPower);
		ps.ManaDelay = (int)PotionManaDelay.ApplyTo(ps.ManaDelay);

		BuffModifierPlayer buffPlayer = player.GetModPlayer<BuffModifierPlayer>();
		buffPlayer.ResistanceStrength = DebuffResistance;
		buffPlayer.BuffBonus = BuffBonus;

		// Apply reflected damage modifier with a base of 0. We need to use "Modifier.Base += x;" for this to be useful at all.
		// We also apply vanilla Thorns buffs in order to make sure we get all the benefit we can.
		ReflectedDamagePlayer reflectPlayer = player.GetModPlayer<ReflectedDamagePlayer>();
		ReflectedDamageModifier *= ReflectedDamagePlayer.GetVanillaThornsModifier(player);
		reflectPlayer.ReflectedDamage = (int)ReflectedDamageModifier.ApplyTo(0);
	}
}

[AttributeUsage(AttributeTargets.Field)]
public class ReverseTooltip : Attribute
{
}