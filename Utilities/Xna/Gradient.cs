#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace PathOfTerraria.Utilities.Xna;

internal struct SingleOrGradient<T>() where T : new()
{
    public T Single = new();
    public Gradient<T>? Gradient;
}

internal struct SingleOrGradient<TSingle, TGradientValue>() where TSingle : new() where TGradientValue : new()
{
    public TSingle Single = new();
    public Gradient<TGradientValue>? Gradient;
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

	public ReadOnlySpan<GradientKey<T>> Keys => CollectionsMarshal.AsSpan(keys);

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
    private Gradient()
    {
        keys = [];
		Verify();
    }

    private static void Verify()
    {
        if (Lerp == null) { throw new NotSupportedException($"Gradient<{typeof(T).Name}>.{nameof(Gradient<float>.Lerp)} is not defined."); }
    }

    public void Add(float time, T value)
    {
        keys.Add(new(time, value));
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

    public T GetValue(float time)
    {
        if (keys.Count == 0) { throw new InvalidOperationException("Gradient length must not be zero."); }

        bool leftDefined = false;
        bool rightDefined = false;
        GradientKey<T> left = default;
        GradientKey<T> right = default;

        for (int i = 0; i < keys.Count; i++)
        {
            if (!leftDefined || keys[i].Time > left.Time && keys[i].Time <= time)
            {
                left = keys[i];
                leftDefined = true;
            }
        }
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (!rightDefined || keys[i].Time < right.Time && keys[i].Time >= time)
            {
                right = keys[i];
                rightDefined = true;
            }
        }

        Debug.Assert(leftDefined && rightDefined);

        return left.Time == right.Time ? left.Value : Lerp!(left.Value, right.Value, (time - left.Time) / (right.Time - left.Time));
    }

    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	public IEnumerator<GradientKey<T>> GetEnumerator()
	{
		return keys.GetEnumerator();
	}
}