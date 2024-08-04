using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Enums;

namespace PathOfTerraria.Common.Data.Models;

public class ItemAffixData
{
	public class TierData
	{
		public float MinValue { get; set; }
		public float MaxValue { get; set; }
		public int MinimumLevel { get; set; }
		public float Weight { get; set; }

		public override string ToString()
		{
			return $"Value Range: {MinValue}-{MaxValue}, Min. Level: {MinimumLevel}, Weight: {Weight}";
		}
	}

	public string AffixType { get; set; }
	public string EquipTypes { get; set; }
	public string Influences { get; set; }
	public List<TierData> Tiers { get; set; }

    public TierData GetAppropriateTierData(int level)
    {
        var eligibleTiers = Tiers.Where(t => t.MinimumLevel <= level).ToList();

        if (eligibleTiers.Count == 0)
        {
            return null;
        }

        float totalWeight = eligibleTiers.Sum(t => t.Weight);

        float randomWeight = (float) Main.rand.NextDouble() * totalWeight;
        float cumulativeWeight = 0;

        foreach (TierData tier in eligibleTiers)
        {
            cumulativeWeight += tier.Weight;

            if (randomWeight <= cumulativeWeight)
            {
                return tier;
            }
        }

        return eligibleTiers.Last(); //Just in case we don't return a tier?
    }

	public ItemType GetEquipTypes()
	{
		string[] split = EquipTypes.Split(' ');
		ItemType types = ItemType.None;

		foreach (string item in split)
		{
			if (Enum.TryParse(item, out ItemType type))
			{
				types |= type;
			}
			else
			{
				Console.WriteLine($"Affix attempted to load nonexisting {item} ItemType enumeration.");
			}
		}

		return types;
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
