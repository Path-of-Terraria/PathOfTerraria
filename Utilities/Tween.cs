#nullable enable

namespace PathOfTerraria.Utilities;

public enum TweenEaseType : byte
{
	Linear,
	CubicInOut,
}

public readonly record struct TweenSegment<T>(T Start, T End, TweenEaseType Ease, int Duration) where T : struct;

/// <summary>Interpolates through a sequence of fixed-duration segments.</summary>
public sealed class Tween<T> where T : struct
{
	public delegate T LerpFunction(T start, T end, float progress);

	private readonly LerpFunction lerp;
	private TweenSegment<T>[] segments = [];
	private int segmentIndex;
	private int elapsed;

	public T Value { get; private set; }
	public bool IsComplete { get; private set; } = true;

	public Tween(LerpFunction lerp, T initialValue = default)
	{
		this.lerp = lerp;
		Value = initialValue;
	}

	public void Start(params TweenSegment<T>[] newSegments)
	{
		if (newSegments.Length == 0)
		{
			throw new ArgumentException("A tween must contain at least one segment.", nameof(newSegments));
		}

		foreach (TweenSegment<T> segment in newSegments)
		{
			if (segment.Duration <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(newSegments), "Tween segment durations must be positive.");
			}
		}

		segments = newSegments;
		segmentIndex = 0;
		elapsed = 0;
		Value = segments[0].Start;
		IsComplete = false;
	}

	public void Update()
	{
		if (IsComplete)
		{
			return;
		}

		TweenSegment<T> segment = segments[segmentIndex];
		elapsed++;

		float progress = MathHelper.Clamp(elapsed / (float)segment.Duration, 0f, 1f);
		progress = segment.Ease switch
		{
			TweenEaseType.CubicInOut => EaseCubicInOut(progress),
			_ => progress,
		};

		Value = lerp(segment.Start, segment.End, progress);

		if (elapsed < segment.Duration)
		{
			return;
		}

		segmentIndex++;
		elapsed = 0;

		if (segmentIndex >= segments.Length)
		{
			Value = segment.End;
			IsComplete = true;
		}
		else
		{
			Value = segments[segmentIndex].Start;
		}
	}

	private static float EaseCubicInOut(float value)
	{
		return value < 0.5f
			? 4f * value * value * value
			: 1f - MathF.Pow((-2f * value) + 2f, 3f) / 2f;
	}
}