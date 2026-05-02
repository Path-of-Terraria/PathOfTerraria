#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PathOfTerraria.Utilities.Xna;

internal struct SingleOrGradient<T>() where T : new()
{
    public T Single = new();
    public Gradient<T>? Gradient = null;
}

internal struct SingleOrGradient<TSingle, TGradientValue>() where TSingle : new() where TGradientValue : new()
{
    public TSingle Single = new();
    public Gradient<TGradientValue>? Gradient = null;
}

internal class Gradient
{
    internal Gradient() { }

    static Gradient()
    {
        Gradient<float>.Lerp = MathHelper.Lerp;
        Gradient<double>.Lerp = (a, b, time) => a + (b - a) * (time < 0d ? 0f : time > 1d ? 1d : time);

        Gradient<int>.Lerp = (left, right, t) => (int)Math.Round(MathHelper.Lerp(left, right, t));
        Gradient<uint>.Lerp = (left, right, t) => (uint)Math.Round(MathHelper.Lerp(left, right, t));
        Gradient<long>.Lerp = (left, right, t) => (long)Math.Round(MathHelper.Lerp(left, right, t));
        Gradient<ulong>.Lerp = (left, right, t) => (ulong)Math.Round(MathHelper.Lerp(left, right, t));

        Gradient<Color>.Lerp = Color.Lerp;

        Gradient<Vector2>.Lerp = Vector2.Lerp;
        Gradient<Vector3>.Lerp = Vector3.Lerp;
        Gradient<Vector4>.Lerp = Vector4.Lerp;
    }
}

public record struct GradientKey<T>(float Time, T Value)
{
	public static implicit operator GradientKey<T>((float time, T value) tuple)
    {
        return new(tuple.time, tuple.value);
    }
}

internal sealed class Gradient<T> : Gradient, IEnumerable<GradientKey<T>>
{
    public delegate T LerpDelegate(T a, T b, float step);

    public static LerpDelegate? Lerp { private get; set; }

    private readonly List<GradientKey<T>> keys;
    private bool needsRecalculation;
    private (float Min, float Max) range;

	public ReadOnlySpan<GradientKey<T>> Keys => CollectionsMarshal.AsSpan(keys);
    public (float Min, float Max) Range
    {
        get
        {
            if (needsRecalculation) { Recalculate(); }
            return range;
        }
    }

	public Gradient(params GradientKey<T>[] values) : this(false, values) { }
    public Gradient(ReadOnlySpan<GradientKey<T>> values) : this(false, values) { }
	public Gradient(bool normalizeTime, params GradientKey<T>[] values)
    {
        keys = values.ToList();
		Verify();
        if (normalizeTime) { Normalize(); }
    }
    public Gradient(bool normalizeTime, ReadOnlySpan<GradientKey<T>> values)
    {
        keys = [.. values];
		Verify();
        if (normalizeTime) { Normalize(); }
    }
    public Gradient(int capacity = 8)
    {
        keys = new(capacity: capacity);
		Verify();
    }

    static void Verify()
    {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        
        if (Lerp == null) { throw new NotSupportedException($"Gradient<{typeof(T).Name}>.{nameof(Gradient<float>.Lerp)} is not defined."); }
    }

    public void Add(float time, T value)
    {
        keys.Add(new(time, value));
        needsRecalculation = true;
    }

    /// <summary> Normalizes all <see cref="Key.Time"/> values to the [0..1] range. </summary>
    public void Normalize()
	{
		float min = keys.Min(k => k.Time);
		float max = keys.Max(k => k.Time);
		float divisor = min != max ? max - min : 1;

		foreach (ref GradientKey<T> key in CollectionsMarshal.AsSpan(keys))
		{
			key.Time = (key.Time - min) / divisor;
		}
	}

    private void Recalculate()
    {
        range = (keys.Min(k => k.Time), keys.Max(k => k.Time));
        needsRecalculation = false;
    }

    public T GetValue(float time)
    {
        if (keys.Count == 0) { throw new InvalidOperationException("Gradient length must not be zero."); }

        GradientKey<T> left = keys[0];
        GradientKey<T> right = keys[^1];

        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].Time > left.Time && keys[i].Time <= time)
            {
                left = keys[i];
            }
        }
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (keys[i].Time < right.Time && keys[i].Time >= time)
            {
                right = keys[i];
            }
        }

        return left.Time != right.Time
            ? Lerp!(left.Value, right.Value, (time - left.Time) / (right.Time - left.Time))
            : left.Value;
    }

    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	public IEnumerator<GradientKey<T>> GetEnumerator()
	{
		return keys.GetEnumerator();
	}
}