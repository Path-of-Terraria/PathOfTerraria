using System.IO;
using Terraria.DataStructures;

namespace PathOfTerraria.Core.IK;

internal record struct IKSegment()
{
	public required float Length;
	public required SpriteFrame Frame;
	/// <summary> The origin to use for rotations, relative to the source rectangle. </summary>
	public required Vector2 Origin;
	/// <summary> The base angle to use in rendering. </summary>
	public float BaseAngle;

	/// <summary> Generates 'points.Length - 1' IK segments using provided origin points. </summary>
	public static IKSegment[] GenerateFromPoints(SpriteFrame baseFrame, ReadOnlySpan<Vector2> points)
	{
		if (points.Length < 2) { throw new ArgumentException("points.Length must be 2 or higher."); }

		var results = new IKSegment[points.Length - 1];
		byte xFrame = 0, yFrame = 0;
		for (int i = 0; i < results.Length; i++)
		{
			results[i] = new()
			{
				Origin = points[i],
				Frame = baseFrame.With(xFrame, yFrame),
				Length = points[i].Distance(points[i + 1]),
				BaseAngle = points[i].AngleTo(points[i + 1]),
			};

			if (++xFrame == baseFrame.ColumnCount)
			{
				xFrame = 0;
				yFrame++;
			}
		}

		return results;
	}
}

internal record struct IKLimbContext
{
	public required Vector2 Center;
	public bool FlipHorizontally;
}

internal record struct IKLimb()
{
	/// <summary> Offset from the center of the body, in pixels. </summary>
	public required Vector2 Offset;
	/// <summary> All of this limb's segments. </summary>
	public required IKSegment[] Segments;
	/// <summary> The resting angle, a slight suggestion for how the limbs should bend. </summary>
	public float RestingAngle = 0f;
	/// <summary> Computed location cache. </summary>
	public Vector2[] Results = [];
	/// <summary> Whether to horizontally flip this limb's rendering. </summary>
	public bool IsFlipped = false;

	public Vector2 Target { get; set; }

	public float SqrLength => Length * length;
	private float length;
	public float Length
	{
		get
		{
			if (length > 0f) { return length; }

			length = 0f;
			foreach (ref readonly IKSegment seg in Segments.AsSpan()) { length += seg.Length; }
			return length;
		}
	}

	public void EnsureReady()
	{
		Array.Resize(ref Results, Segments.Length + 1);
	}

	// public void SetTarget(Vector2 target, bool immediate = false)
	// {
	//     TargetWanted = target;
	//     if (immediate) { TargetCurrent = target; }
	// }

	/// <summary> Calculates the limb's body-relative origin point for the given body angle and directions. </summary>
	public readonly Vector2 GetOffset(float rotation = 0f, int xDir = 1, int yDir = 1)
	{
		var result = new Vector2(Offset.X * xDir, Offset.Y * yDir);
		if (rotation != 0f) { result = result.RotatedBy(rotation); }
		return result;
	}
	/// <summary> Calculates the limb's origin point for the given body transformation. </summary>
	public readonly Vector2 GetPosition(Vector2 bodyCenter, float rotation = 0f, int xDir = 1, int yDir = 1)
	{
		return bodyCenter + GetOffset(rotation, xDir, yDir);
	}

	public void Reset(in IKLimbContext ctx)
	{
		EnsureReady();

		Span<InverseKinematics.Segment> segments = new InverseKinematics.Segment[Segments.Length];
		// Span<InverseKinematics.Segment> segments = stackalloc InverseKinematics.Segment[Segments.Length];
		for (int i = 0; i < Segments.Length; i++) { segments[i] = new(Segments[i].Length, Segments[i].BaseAngle); }

		InverseKinematics.Reset(new()
		{
			Target = Target,
			Origin = ctx.Center,
			Results = Results,
			Segments = segments,
			TotalLength = Length,
			FlipHorizontally = ctx.FlipHorizontally,
		});
	}

	public void Resolve(out InverseKinematics.Info info, in InverseKinematics.Config cfg, in IKLimbContext ctx)
	{
		EnsureReady();

		Span<InverseKinematics.Segment> segments = new InverseKinematics.Segment[Segments.Length];
		// Span<InverseKinematics.Segment> segments = stackalloc InverseKinematics.Segment[Segments.Length];
		for (int i = 0; i < Segments.Length; i++) { segments[i] = new(Segments[i].Length, Segments[i].BaseAngle); }

		InverseKinematics.Resolve(out info, in cfg, new()
		{
			Target = Target,
			Origin = ctx.Center,
			Results = Results,
			Segments = segments,
			TotalLength = Length,
			FlipHorizontally = ctx.FlipHorizontally,
		});
	}

	public void Quantize(float snapInRadians)
	{
		EnsureReady();
		InverseKinematics.Quantize(Results, snapInRadians);
	}

	public DrawData GetDrawParams(Texture2D texture, int segmentIdx, Vector2 screenPos, int direction)
	{
		EnsureReady();

		ref readonly IKSegment segment = ref Segments[segmentIdx];
		(Vector2 start, Vector2 end) = (Results[segmentIdx], Results[segmentIdx + 1]);
		float angle = start.AngleTo(end);

		Vector2 origin;
		float spriteAngle;
		SpriteEffects effects;
		Color color = Color.White;
		// Color color = Lighting.GetColor(((start + end) * 0.5f).ToTileCoordinates());

		Rectangle srcRect = segment.Frame.GetSourceRectangle(texture);

		bool flipped = ((IsFlipped ? 1 : -1) * direction) < 0;
		if (!flipped)
		{
			origin = segment.Origin;
			spriteAngle = angle - segment.BaseAngle;
			effects = 0;
		}
		else
		{
			origin = new Vector2(srcRect.Width - segment.Origin.X, segment.Origin.Y);
			spriteAngle = angle + segment.BaseAngle + MathHelper.Pi;
			effects = SpriteEffects.FlipHorizontally;
		}

		return new(texture, start - screenPos, srcRect, color, spriteAngle, origin, 1f, effects);
	}
}
