using System.Collections.Generic;
using System.Linq;
using Terraria.Utilities;

#pragma warning disable IDE0290 // Use primary constructor

namespace PathOfTerraria.Utilities.Terraria;

/// <summary> An improved version of vanilla's WeightedRandom<T> collection. </summary>
public struct WeightRand<TValue>
{
    public List<(TValue Value, double Weight)> Items { get; set; }
    public UnifiedRandom Random { get; set; } = Main.rand;
    private bool isDirty = true;
    private double totalWeight;

    public WeightRand(params (TValue Value, double Weight)[] items) : this(Main.rand, items) { }
    public WeightRand(List<(TValue Value, double Weight)> items) : this(Main.rand, items) { }
    public WeightRand(UnifiedRandom random, params (TValue Value, double Weight)[] elements) : this(random, elements.ToList()) { }
    public WeightRand(UnifiedRandom random, List<(TValue Value, double Weight)> elements)
    {
        Random = random;
        Items = elements;
    }
    public WeightRand(int capacity)
    {
        Items = new(capacity);
    }

    public void Add(TValue element, double weight = 1.0)
    {
        Items.Add((element, weight));
        isDirty = true;
    }
    public void MarkDirty()
    {
        isDirty = true;
    }
    public void Clear()
    {
        Items.Clear();
        MarkDirty();
    }

    public double GetTotalWeight()
    {
        if (isDirty)
        {
            totalWeight = Items.Sum(it => it.Weight);
            isDirty = false;
        }

        return totalWeight;
    }

    public TValue RollValue()
    {
        return Items[RollIndex()].Value;
    }
    public int RollIndex()
    {
        double num = Random.NextDouble();
        num *= GetTotalWeight();
        
        for (int i = 0; i < Items.Count; i++)
        {
            (TValue value, double weight) = Items[i];
            if (num > weight)
            {
                num -= weight;
                continue;
            }

            return i;
        }

        return default;
    }
    /// <summary> Rolls an index, removes it from the list, and returns a copy of its value. </summary>
    public TValue PopValue()
    {
        int index = RollIndex();
        TValue value = Items[index].Value;
        Items.RemoveAt(index);
        return value;
    }
}
