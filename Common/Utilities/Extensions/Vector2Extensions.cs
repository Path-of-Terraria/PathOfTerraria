namespace PathOfTerraria.Common.Utilities.Extensions;

public static class Vector2Extensions
{
	public static Vector2 RotateTowards(this Vector2 currentDirection, Vector2 targetDirection, float maxRadiansDelta)
	{
		float angleCurrent = (float)Math.Atan2(currentDirection.Y, currentDirection.X);
		float angleTarget = (float)Math.Atan2(targetDirection.Y, targetDirection.X);

		float angleDifference = MathHelper.WrapAngle(angleTarget - angleCurrent);

		if (Math.Abs(angleDifference) <= maxRadiansDelta)
		{
			return targetDirection;
		}

		angleDifference = MathHelper.Clamp(angleDifference, -maxRadiansDelta, maxRadiansDelta);

		float newAngle = angleCurrent + angleDifference;
		return new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * currentDirection.Length();
	}
}