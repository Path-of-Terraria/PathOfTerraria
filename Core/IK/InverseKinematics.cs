namespace PathOfTerraria.Core.IK;

/// <summary> Implementation and utilities for simple 2D IK position resolution. </summary>
internal static class InverseKinematics
{
    public ref struct Context()
    {
        public required Vector2 Target;
        public required Vector2 Origin;
        public required Span<Vector2> Results;
        public required ReadOnlySpan<float> Segments;
        public required float TotalLength;
        public float RestingAngle = 0f;
    }

    public struct Config()
    {
        public int NumIterations = 8;
        public float CloseEnoughRange = 0.1f;
        /// <summary> When more than zero, a pass will be ran to reduce the resolution of limbs' angles to this radian value.
        /// e.g. specifying Pi/2 will result in 90-degree snaps. </summary>
        public float RadianQuantization;
    }

    public struct Info()
    {
        public bool TargetInRange;
    }

    public static void Resolve(out Info info, in Config cfg, in Context ctx)
    {
        info = new();

        if (ctx.Segments.Length == 0) { return; }
        if (ctx.Results.Length <= ctx.Segments.Length) { throw new ArgumentException($"Results span must be larger than segments/lengths."); }

        // If the target is beyond our reach - just stretch all segments towards it.
        var totalDiff = (Vector2)(ctx.Target - ctx.Origin);
        if (totalDiff.LengthSquared() >= ctx.TotalLength * ctx.TotalLength)
        {
            (Vector2 dir, Vector2 nextPos) = (totalDiff.SafeNormalize(Vector2.UnitX), ctx.Origin);
            for (int j = 0; j < ctx.Results.Length; j++)
            {
                ctx.Results[j] = nextPos;
                if (j != ctx.Segments.Length) { nextPos += dir * (ctx.Segments[j]); }
            }

            info.TargetInRange = false;
            return;
        }

        info.TargetInRange = true;

        // Reset positions.
        Vector2 restingDir = ctx.RestingAngle.ToRotationVector2();
        for (int j = 0; j < ctx.Results.Length; j++)
        {
            ctx.Results[j] = j != 0 ? (ctx.Results[j - 1] + (restingDir * ctx.Segments[j - 1])) : ctx.Origin;
        }

        // Run IK iterations.
        for (int it = 0; it < cfg.NumIterations; it++)
        {
            // End-to-Start pass.
            ctx.Results[^1] = ctx.Target;
            for (int j = ctx.Results.Length - 2; j > 0; j--)
            {
                ctx.Results[j] = ctx.Results[j + 1] + (ctx.Results[j + 1].DirectionTo(ctx.Results[j])) * ctx.Segments[j];
            }

            // Start-to-End pass.
            for (int j = 1; j < ctx.Results.Length; j++)
            {
                ctx.Results[j] = ctx.Results[j - 1] + (ctx.Results[j - 1].DirectionTo(ctx.Results[j])) * ctx.Segments[j - 1];
            }

            // Short-circuit if this point is close enough.
            if (ctx.Results[^1].WithinRange(ctx.Target, cfg.CloseEnoughRange))
            {
                break;
            }
        }

        if (cfg.RadianQuantization > 0f)
        {
            Quantize(ctx.Results, snapInRadians: cfg.RadianQuantization);
        }
    }

    /// <summary> Reduces the resolution of angles between the provided points using the provided radian angle value.
    /// e.g. Specifying Pi/2 will result in 90-degree snaps. </summary>
    public static void Quantize(Span<Vector2> points, float snapInRadians)
    {
        for (int j = 1; j < points.Length; j++)
        {
            (Vector2 start, Vector2 end) = (points[j - 1], points[j]);
            float length = start.Distance(end);
            float oldAngle = start.AngleTo(end);
            float newAngle = MathF.Round(oldAngle / snapInRadians) * snapInRadians;
            points[j] = start + newAngle.ToRotationVector2() * length;
        }
    }
}
