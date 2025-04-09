namespace PathOfTerraria.Core.Physics.Verlet;

public sealed class VerletPoint(Vector2 position, bool pinned)
{
	/// <summary>
	///		Gets whether the point is pinned.
	/// </summary>
	public bool Pinned { get; } = pinned;

	/// <summary>
	///		Gets or sets the velocity of the point.
	/// </summary>
	public Vector2 Velocity;

	/// <summary>
	///		Gets or sets the position of the point.
	/// </summary>
	public Vector2 Position = position;

	/// <summary>
	///		Gets or sets the old position of the point.
	/// </summary>
	public Vector2 OldPosition = position;
}