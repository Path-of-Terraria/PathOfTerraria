using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.ItemDropping;

namespace PathOfTerraria.Common.Data.Models;

public class ItemAffixData
{
	public class TierData
	{
		public float MinValue { get; set; }
		public float MaxValue { get; set; }
		public int MinimumLevel { get; set; }
		public float Weight { get; set; }

		/// <summary>
		/// Used exclusively for <see cref="Systems.Affixes.ItemTypes.MapAffix"/>es.
		/// </summary>
		public float Strength { get; set; }

		public override string ToString()
		{
			return $"Value Range: {MinValue}-{MaxValue}, Min. Level: {MinimumLevel}, Weight: {Weight}";
		}
	}

	public string AffixType { get; set; }
	public string EquipTypes { get; set; }
	public string Influences { get; set; }
	public bool Round { get; set; }
	public List<TierData> Tiers { get; set; }

	public (int MinInclusive, int MaxInclusive) GetPossibleTierRange(int level)
	{
		var eligibleTiers = Tiers.Where(t => t.MinimumLevel <= level).ToList();

		return (0, eligibleTiers.Count - 1);
	}

	public TierData GetAppropriateTierData(int level, out int tierIndex)
	{
		var eligibleTiers = Tiers.Where(t => t.MinimumLevel <= level).ToList();

		tierIndex = 0;

		if (eligibleTiers.Count == 0)
		{
			return null;
		}

		// Smart-loot bias: re-weights eligible tiers toward higher indices, where index 0 is the
		// lowest tier the item level qualifies for. Inactive (bias == 0) outside chest population.
		float bias = SmartLoot.TierBias;
		float[] weights = new float[eligibleTiers.Count];
		float totalWeight = 0f;

		for (int i = 0; i < eligibleTiers.Count; i++)
		{
			float w = eligibleTiers[i].Weight;

			if (bias > 0f)
			{
				w *= MathF.Pow(1f + bias, i);
			}

			weights[i] = w;
			totalWeight += w;
		}

		float randomWeight = (float)Main.rand.NextDouble() * totalWeight;
		float cumulativeWeight = 0;

		for (int i = 0; i < eligibleTiers.Count; i++)
		{
			cumulativeWeight += weights[i];

			if (randomWeight <= cumulativeWeight)
			{
				tierIndex = i;
				return eligibleTiers[i];
			}
		}

		tierIndex = eligibleTiers.Count - 1;
		return eligibleTiers.Last(); //Just in case we don't return a tier?
	}

	public ItemType GetEquipTypes()
	{
		ItemType result = 0;

		foreach (string item in EquipTypes.Split(' '))
		{
			bool? negate = item.StartsWith('-') ? true : (item.StartsWith('+') ? false : null);
			ReadOnlySpan<char> chars = item.AsSpan(negate.HasValue ? 1 : 0);
			if (Enum.TryParse(chars, out ItemType type))
			{
				result = negate != true ? (result | type) : (result &= ~type);
				continue;
			}

			string msg = $"Affix attempted to load non-existing '{item}' ItemType enumeration. Types: {EquipTypes}\n{Environment.StackTrace}";
			PoTMod.Instance.Logger.Error(msg);
			Debug.Fail(msg);
		}

		return result;
	}

	public Influence GetInfluences()
	{
		if (Influences is null || Influences == string.Empty)
		{
			return Influence.None;
		}

		string[] split = Influences.Split(' ');
		Influence types = Influence.None;

		foreach (string item in split)
		{
			if (Enum.TryParse(item, out Influence type))
			{
				types |= type;
			}
			else
			{
				Console.WriteLine($"Affix attempted to load nonexisting {item} Influence enumeration.");
			}
		}

		return types;
	}

	public override string ToString()
	{
		return $"Type: {AffixType}, Equips: {EquipTypes}, Influences: {Influences}";
	}
}
