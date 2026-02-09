using Terraria.DataStructures;

namespace PathOfTerraria.Core.IK;

internal record struct IKSegment()
{
	public required float Length;
	public required Rectangle SrcRect;
	/// <summary> The origin to use for rotations, relative to the whole texture rather than the source rectangle. </summary>
	public required Vector2 TextureOrigin;
	/// <summary> The base angle to use in rendering. </summary>
	public float BaseAngle;
}

internal record struct IKLimbContext
{
	public required Vector2 Center;
	public required int Direction;
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

    public Vector2 TargetCurrent { get; set; }
    public Vector2 TargetWanted { get; set; }

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

	public readonly Vector2 GetOffset(float rotation = 0f, int xDir = 1, int yDir = 1)
	{
		var result = new Vector2(Offset.X * xDir, Offset.Y * yDir);
		if (rotation != 0f) { result = result.RotatedBy(rotation); }
		return result;
	}
	public readonly Vector2 GetPosition(Vector2 bodyCenter, float rotation = 0f, int xDir = 1, int yDir = 1)
	{
		return bodyCenter + GetOffset(rotation, xDir, yDir);
	}

    public void Resolve(out InverseKinematics.Info info, in InverseKinematics.Config cfg, in IKLimbContext ctx)
    {
        EnsureReady();
        
		Span<float> lengths = stackalloc float[Segments.Length];
		for (int i = 0; i < Segments.Length; i++) { lengths[i] = Segments[i].Length; }
	
		InverseKinematics.Resolve(out info, in cfg, new()
		{
            Target = TargetCurrent,
			Origin = ctx.Center,
			Results = Results,
			Segments = lengths,
			TotalLength = Length,
			RestingAngle = RestingAngle + (ctx.Direction < 0 ? -MathHelper.Pi : 0f),
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

		bool flipped = ((IsFlipped ? 1 : -1) * direction) < 0;
		if (!flipped)
		{
            origin = segment.TextureOrigin - segment.SrcRect.TopLeft();
			spriteAngle = angle + segment.BaseAngle;
			effects = 0;
		}
        else
		{
            origin = segment.TextureOrigin - segment.SrcRect.TopLeft();
			origin.X = segment.SrcRect.Width - origin.X;
			spriteAngle = angle - segment.BaseAngle + MathHelper.Pi;
			effects = SpriteEffects.FlipHorizontally;
		}
        
        return new(texture, start - screenPos, segment.SrcRect, color, spriteAngle, origin, 1f, effects);
    }
}
