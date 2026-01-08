namespace PathOfTerraria.Utilities.Terraria;

internal static class MovementUtils
{
	/// <summary> Quake-like acceleration towards a direction. </summary>
	public static Vector2 DirAccel(Vector2 velocity, Vector2 wishDirection, float wishSpeed, float acceleration)
	{
		Vector2 wishVelocity = wishDirection * wishSpeed;

		Vector2 pushDifference = wishVelocity - velocity;
		float pushLength = pushDifference.Length();
		Vector2 pushDirection = pushLength > 0f ? (pushDifference / pushLength) : default;

		float accelerationSpeed = MathF.Min(acceleration * wishSpeed, pushLength);
		velocity += pushDirection * accelerationSpeed;

		return velocity;
	}

	/// <summary> Quake-like acceleration towards a direction. </summary>
	public static Vector2 DirAccelQ(Vector2 velocity, Vector2 wishDirection, float wishSpeed, float acceleration)
	{
		float currentSpeed = Vector2.Dot(velocity, wishDirection);
		float addSpeed = wishSpeed - currentSpeed;

		if (addSpeed > 0f)
		{
			float accelSpeed = Math.Min(acceleration * wishSpeed, addSpeed);
			velocity += accelSpeed * wishDirection;
		}

		return velocity;
	}
}
