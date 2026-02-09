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

internal record struct IKLimb()
{
	/// <summary> Offset from the center of the body, in pixels. </summary>
	public required Vector2 Offset;
	/// <summary> All of this limb's segments. </summary>
	public required IKSegment[] Segments;
	/// <summary> Computed location cache. </summary>
	public Vector2[] Results = [];
	/// <summary> Whether to horizontally flip this limb's rendering. </summary>
	public bool IsFlipped = false;

    public Vector2 TargetCurrent { get; set; }
    public Vector2 TargetWanted { get; set; }

    public void EnsureReady()
    {
        Array.Resize(ref Results, Segments.Length + 1);
    }

    // public void SetTarget(Vector2 target, bool immediate = false)
    // {
    //     TargetWanted = target;
    //     if (immediate) { TargetCurrent = target; }
    // }

    public void Resolve(Vector2 bodyCenter, out InverseKinematics.Info info, in InverseKinematics.Config cfg)
    {
        EnsureReady();
        
		Span<float> lengths = stackalloc float[Segments.Length];
        float totalLength = 0f;
		for (int i = 0; i < Segments.Length; i++)
        {
            lengths[i] = Segments[i].Length;
            totalLength += lengths[i];
        }
	
		InverseKinematics.Resolve(out info, in cfg, new()
		{
            Target = TargetCurrent,
			Origin = bodyCenter + Offset,
			Results = Results,
			Segments = lengths,
			TotalLength = totalLength,
		});
    }

    public void Quantize(float snapInRadians)
    {
        EnsureReady();
        InverseKinematics.Quantize(Results, snapInRadians);
    }

    public DrawData GetDrawParams(Texture2D texture, int segmentIdx, Vector2 screenPos)
    {
        EnsureReady();

        ref readonly IKSegment segment = ref Segments[segmentIdx];
		(Vector2 start, Vector2 end) = (Results[segmentIdx], Results[segmentIdx + 1]);
		float angle = start.AngleTo(end);

		Vector2 origin;
		float spriteAngle;
		SpriteEffects effects;
		Color color = Lighting.GetColor(((start + end) * 0.5f).ToTileCoordinates());

		if (!IsFlipped)
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
