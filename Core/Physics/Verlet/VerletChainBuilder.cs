namespace PathOfTerraria.Core.Physics.Verlet;

/// <summary>
///		Provides utility methods for generating common verlet chain shapes.
/// </summary>
public static class VerletChainBuilder
{
	/// <summary>
	///		Creates a pinned rope verlet chain shape.
	/// </summary>
	/// <param name="origin">The starting position of the rope, in world coordinates.</param>
	/// <param name="segments">The number of segments in the rope.</param>
	/// <param name="length">The length of each segment in the rope.</param>
	/// <param name="friction">The friction applied to the chain. Defaults to <c>0.99f</c>.</param>
	/// <param name="gravity">The gravity applied to the chain. Defaults to <c>0.3f</c>.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="segments"/> is less than or equal to zero.</exception>
	/// <returns>An instance of <see cref="VerletChain"/> with the generated shape.</returns>
	public static VerletChain CreatePinnedRope(Vector2 origin, int segments, float length, float friction = 0.99f, float gravity = 0.3f)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(segments, nameof(segments));
		
		var chain = new VerletChain(friction, gravity, segments);

		for (var i = 0; i < segments; i++)
		{
			var point = new VerletPoint(origin, i == 0);
			
			chain.AddPoint(in point);
		}
		
		for (var i = 0; i < segments - 1; i++)
		{
			var stick = new VerletStick(chain.Points[i], chain.Points[i + 1], length);
			
			chain.AddStick(in stick);
		}
		
		return chain;
	}
}