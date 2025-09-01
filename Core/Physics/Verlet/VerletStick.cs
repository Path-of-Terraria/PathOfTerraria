using Terraria.GameContent;

namespace PathOfTerraria.Core.Physics.Verlet;

public sealed class VerletStick(VerletPoint start, VerletPoint end, float length)
{
	/// <summary>
	///		Gets the starting point of the stick.
	/// </summary>
	public VerletPoint Start = start;

	/// <summary>
	///		Gets the ending point of the stick.
	/// </summary>
	public VerletPoint End = end;

	/// <summary>
	///		Gets the length of the stick. Shorthand for <c>Vector2.Distance(Start.Position, End.Position)</c>.
	/// </summary>
	public float Length { get; } = length;
}