using PathOfTerraria.Core.Systems.ModPlayers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PathOfTerraria.Core.Systems;

internal partial class EntityModifier
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
	public StatModifier OnFireChance = new();

	// MinorStatsModPlayer:
	public StatModifier MagicFind = new();

	// PotionSystem:
	public StatModifier MaxHealthPotions = new();
	public StatModifier PotionHealPower = new();
	public StatModifier PotionHealDelay = new();

	public StatModifier MaxManaPotions = new();
	public StatModifier PotionManaPower = new();
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
	
		UniversalBuffingPlayer universalBuffingPlayer = player.GetModPlayer<UniversalBuffingPlayer>();
		universalBuffingPlayer.OnFireChance = OnFireChance;
    
    BuffModifierPlayer buffPlayer = player.GetModPlayer<BuffModifierPlayer>();
		buffPlayer.ResistanceStrength = DebuffResistance;
		buffPlayer.BuffBonus = BuffBonus;

		// Apply reflected damage modifier with a base of 0. We need to use "Modifier.Base += x;" for this to be useful at all.
		// We also apply vanilla Thorns buffs in order to make sure we get all the benefit we can.
		ReflectedDamagePlayer reflectPlayer = player.GetModPlayer<ReflectedDamagePlayer>();
		ReflectedDamageModifier *= ReflectedDamagePlayer.GetVanillaThornsModifier(player);
		reflectPlayer.ReflectedDamage = (int)ReflectedDamageModifier.ApplyTo(0);
	}

	private readonly FieldInfo[] _fields =
		typeof(EntityModifier).GetFields().Where(f => f.FieldType == typeof(StatModifier)).ToArray();

	public List<string> GetDifference(EntityModifier other)
	{
		List<string> strings = [];

		for (int i = 0; i < _fields.Length; i++)
		{
			StatModifier thisField = (StatModifier)_fields[i].GetValue(this);
			StatModifier otherField = (StatModifier)_fields[i].GetValue(other);

			if (thisField != otherField)
			{
				strings.AddRange(GetDifferences(thisField, otherField)
					.Select(s => s.Replace("#", DifferenceRegex().Replace(_fields[i].Name, "$1 $2"))));
			}
		}

		return strings;
	}

	public static List<string> GetChange(EntityModifier changed) { return _default.GetDifference(changed); }

	private string[] GetDifferences(StatModifier m1, StatModifier m2)
	{
		List<string> differences = [];

		float baseDiff = m2.Base - m1.Base;
		if (baseDiff != 0)
		{
			string type = "+";
			if (baseDiff < 0)
			{
				type = "";
			}

			differences.Add($"{type}{MathF.Round(baseDiff, 2)} base #");
		}

		float addDiff = m2.Additive - m1.Additive;
		if (addDiff != 0)
		{
			string type = "+";
			if (addDiff < 0)
			{
				type = "";
			}

			differences.Add($"{type}{MathF.Round(addDiff * 100f, 2)}% #");
		}

		float multDiff = m2.Multiplicative - m1.Multiplicative;
		if (multDiff != 0)
		{
			string type = "increased";
			if (multDiff < 0)
			{
				type = "decreased";
			}

			differences.Add($"{MathF.Abs(MathF.Round(multDiff * 100f, 2))}% {type} #");
		}

		float flatDiff = m2.Flat - m1.Flat;
		if (flatDiff != 0)
		{
			string type = "+";
			if (flatDiff < 0)
			{
				type = "";
			}

			differences.Add($"{type}{MathF.Round(flatDiff, 2)} flat #");
		}

		return [.. differences];
	}

	[GeneratedRegex("([a-z])([A-Z])")]
	private static partial Regex DifferenceRegex();
}