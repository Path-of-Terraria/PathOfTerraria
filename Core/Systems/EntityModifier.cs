using PathOfTerraria.Core.Systems.ModPlayers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PathOfTerraria.Core.Systems;
internal class EntityModifier
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

	public void ApplyTo(NPC npc)
	{
		npc.life = (int)MaximumLife.ApplyTo(npc.life);
		npc.lifeRegen = (int)LifeRegen.ApplyTo(npc.lifeRegen);
		npc.defense = (int)Defense.ApplyTo(npc.defense); // npcs have no such thing as damage reduction, idk how we apply that.
														 //	Can only think of a global npc.
		npc.damage = (int)Damage.ApplyTo(npc.damage);

		// there are many things that would need a global npc to be applied.
	}

	public void ApplyTo(Player player)
	{
		player.statLifeMax2 = (int)MaximumLife.ApplyTo(player.statLifeMax2);

		player.lifeRegen = (int)LifeRegen.ApplyTo(player.lifeRegen); // dont know if this is the right value
		player.statManaMax2 = (int)MaximumMana.ApplyTo(player.statManaMax2);
		// player.statManaMax - for when the overlay gets implimented

		player.manaRegen = (int)ManaRegen.ApplyTo(player.manaRegen); // dont know if this is the right value
		player.statDefense += (int)Defense.ApplyTo((int)player.statDefense) - (int)player.statDefense;
		
		player.endurance *= (int)DamageReduction.ApplyTo(player.endurance); // i think this is right..?
		player.moveSpeed = MovementSpeed.ApplyTo(player.moveSpeed);
		player.GetDamage(DamageClass.Generic) = player.GetDamage(DamageClass.Generic).CombineWith(Damage);

		player.GetAttackSpeed(DamageClass.Generic) = Attackspeed.ApplyTo(player.GetAttackSpeed(DamageClass.Generic));
		
		player.GetKnockback(DamageClass.Generic) = player.GetKnockback(DamageClass.Generic).CombineWith(Knockback);
		player.GetArmorPenetration(DamageClass.Generic) = Knockback.ApplyTo(player.GetArmorPenetration(DamageClass.Generic));

		MinorStatsModPlayer msmp = player.GetModPlayer<MinorStatsModPlayer>();
		msmp.MagicFind = MagicFind.ApplyTo(msmp.MagicFind);

		PotionSystem ps = player.GetModPlayer<PotionSystem>();
		ps.MaxHealing = (int)MaxHealthPotions.ApplyTo(ps.MaxHealing);
		ps.HealPower = (int)PotionHealPower.ApplyTo(ps.HealPower);
		ps.HealDelay = (int)PotionHealDelay.ApplyTo(ps.HealDelay);

		ps.MaxMana = (int)MaxHealthPotions.ApplyTo(ps.MaxMana);
		ps.ManaPower = (int)PotionManaPower.ApplyTo(ps.ManaPower);
		ps.ManaDelay = (int)PotionManaDelay.ApplyTo(ps.ManaDelay);
	}

	private readonly FieldInfo[] _fields = typeof(EntityModifier).GetFields().Where(f => f.FieldType == typeof(StatModifier)).ToArray();
	public List<Tuple<string, bool>> GetDifference(EntityModifier other)
	{
		List<Tuple<string, bool>> strings = new List<Tuple<string, bool>>();

		for (int i = 0; i < _fields.Length; i++)
		{
			StatModifier thisField = (StatModifier)_fields[i].GetValue(this);
			StatModifier otherField = (StatModifier)_fields[i].GetValue(other);

			if (thisField != otherField)
			{
				strings.AddRange(GetDifferences(thisField, otherField, _fields[i].GetCustomAttribute<ReverseTooltip>() is not null)
					.Select(s => new Tuple<string, bool>(s.Item1.Replace("#", Regex.Replace(_fields[i].Name, "([a-z])([A-Z])", "$1 $2")), s.Item2)));
			}
		}

		return strings;
	}
	public List<string> GetDifferenceOnlyStrings(EntityModifier other)
	{
		return GetDifference(other).Select(s => s.Item1).ToList();
	}

	public static List<Tuple<string, bool>> GetChange(EntityModifier changed) { return _default.GetDifference(changed); }
	public static List<string> GetChangeOnlyStrings(EntityModifier changed) { return _default.GetDifferenceOnlyStrings(changed); }
	private List<Tuple<string, bool>> GetDifferences(StatModifier m1, StatModifier m2, bool reversed = false)
	{
		List<Tuple<string, bool>> differences = new List<Tuple<string, bool>>();

		float baseDiff = m2.Base - m1.Base;
		if (baseDiff != 0)
		{
			string type = "+";
			if (baseDiff < 0)
			{
				type = "";
			}

			bool isPositive = type == "+";
			differences.Add(new($"{type}{MathF.Round(baseDiff, 2)} base #", reversed ? !isPositive : isPositive));
		}

		float addDiff = m2.Additive - m1.Additive;
		if (addDiff != 0)
		{
			string type = "+";
			if (addDiff < 0)
			{
				type = "";
			}

			bool isPositive = type == "+";
			differences.Add(new($"{type}{MathF.Round(addDiff * 100f, 2)}% #", reversed ? ! : isPositive));
		}

		float multDiff = m2.Multiplicative - m1.Multiplicative;
		if (multDiff != 0)
		{
			string type = "increased";
			if (multDiff < 0)
			{
				type = "decreased";
			}
			bool isPositive = type == "increased";

			differences.Add(new($"{MathF.Abs(MathF.Round(multDiff * 100f, 2))}% {type} #", reversed ? !isPositive : isPositive));
		}

		float flatDiff = m2.Flat - m1.Flat;
		if (flatDiff != 0)
		{
			string type = "+";
			if (flatDiff < 0)
			{
				type = "";
			}

			bool isPositive = type == "+";
			differences.Add(new($"{type}{MathF.Round(flatDiff, 2)} flat #", reversed ? !isPositive : isPositive));
		}

		return differences;
	}
}

public class ReverseTooltip : Attribute {}