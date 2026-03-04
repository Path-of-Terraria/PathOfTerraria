#define TERRARIA
#define XNA

using System.Numerics;

namespace PathOfTerraria.Utilities;

/// <summary> Sums weighted values in manner of: <code>(weight += w, value += (v * w))</code> </summary>
internal struct WeightedValue<TValue, TWeight>(TValue Value, TWeight Weight, TWeight MinWeight = default)
	where TWeight : INumber<TWeight>
	where TValue : IAdditionOperators<TValue, TValue, TValue>, IMultiplyOperators<TValue, TWeight, TValue>, IDivisionOperators<TValue, TWeight, TValue>
{
	public TValue TotalValue { get; private set; } = Value;
	public TWeight TotalWeight { get; private set; } = Weight;

	public void Add(TValue value, TWeight weight)
	{
		TotalWeight += weight;
		TotalValue += (value * weight);
	}

	public readonly TValue Total()
	{
		if (TWeight.IsZero(TotalWeight)) { return TotalValue; }

		return TotalValue / (TWeight.Max(MinWeight, TotalWeight));
	}
}

/// <inheritdoc cref="WeightedValue{TValue, TWeight}"/>
internal struct WeightedValue2D<TValue, TWeight>((TValue X, TValue Y) value, (TWeight X, TWeight Y) weight, (TWeight X, TWeight Y) minWeight = default)
	where TWeight : INumber<TWeight>
	where TValue : IAdditionOperators<TValue, TValue, TValue>, IMultiplyOperators<TValue, TWeight, TValue>, IDivisionOperators<TValue, TWeight, TValue>
{
	public WeightedValue<TValue, TWeight> X = new(value.X, weight.X, minWeight.X);
	public WeightedValue<TValue, TWeight> Y = new(value.Y, weight.Y, minWeight.Y);

	public void Add((TValue X, TValue Y) value, (TWeight X, TWeight Y) weight)
	{
		X.Add(value.X, weight.X);
		Y.Add(value.Y, weight.Y);
	}

	public readonly (TValue X, TValue Y) Total()
	{
		return (X.Total(), Y.Total());
	}
}

