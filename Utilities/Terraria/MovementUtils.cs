using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Utilities.Terraria;

internal static class MovementUtils
{
	/// <summary> Linearly deaccelerates given velocity by the given value if its speed is faster than the given cap. </summary>
	public static Vector2 ApplySoftSpeedCap(Vector2 velocity, float softCap, float deceleration)
	{
		if (velocity.Length() is float speed && speed > softCap)
		{
			float newSpeed = MathUtils.StepTowards(speed, softCap, deceleration);
			velocity = (velocity / speed) * newSpeed;
		}

		return velocity;
	}
	/// <summary> Clamps the given velocity by the given upper boundary. </summary>
	public static Vector2 ApplyHardSpeedCap(Vector2 velocity, float hardCap)
	{
		if (velocity.Length() is float speed && speed > hardCap)
		{
			velocity *= hardCap / speed;
		}

		return velocity;
	}
	
	/// <summary> Acceleration towards a direction. </summary>
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

	/// <summary> Quake-like acceleration towards a direction, generates abusable extra velocity when turning. </summary>
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
