namespace PathOfTerraria.Common.NPCs;

/// <summary>
/// Copied from Spirit Mod. Helps with arcing velocities.
/// </summary>
internal static class ArcHelper
{
	public static Vector2 GetArcVel(Vector2 startingPos, Vector2 targetPos, float gravity, float speed, bool useHigherAngle = false)
	{
		//Start by getting the desired end point relative to the starting point, and the rotation to that end point
		Vector2 distToTravel = targetPos - startingPos;
		float angle = distToTravel.ToRotation();

		while (true)
		{
			float finalX = Math.Abs(distToTravel.X);
			float finalY = distToTravel.Y;

			//The function below is written assuming a x distance to travel above 0- If it is zero, just return an angle either directly up or down based on the target point
			if (finalX == 0)
			{
				angle = finalY >= 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
				break;
			}

			float quadFormulaB = 2 * (float)Math.Pow(speed, 2) / (gravity * finalX);
			float quadFormulaC = 1 - quadFormulaB / finalX * finalY;

			//Check if the projectile can even reach the target, if not, move the target closer until it can
			if (QuadFormulaSolvable(1, quadFormulaB, quadFormulaC))
			{
				float[] tanTheta = SolveQuadraticFormula(1, quadFormulaB, quadFormulaC);
				float[] theta = [(float)Math.Atan(tanTheta[0]), (float)Math.Atan(tanTheta[1])];

				//Flip the angle horizontally if the original trajectory was to the left
				if (distToTravel.X < 0)
				{
					for (int i = 0; i < theta.Length; i++)
					{
						theta[i] = FlipAngle(theta[i]);
					}
				}

				//Choose the angle that's closer to the original angle
				if (Math.Abs(AngleDistance(angle, theta[0])) > Math.Abs(AngleDistance(angle, theta[1])))
				{
					angle = useHigherAngle ? theta[0] : theta[1];
				}
				else
				{
					angle = useHigherAngle ? theta[1] : theta[0];
				}

				break;
			}

			float distanceDecrease = 10;
			distToTravel -= distanceDecrease * Vector2.Normalize(distToTravel);
		}

		return Vector2.UnitX.RotatedBy(angle) * speed;
	}

	public static Vector2 GetArcVel(this Entity ent, Vector2 targetPos, float gravity, float speed, bool useHigherAngle = false)
	{
		return GetArcVel(ent.Center, targetPos, gravity, speed, useHigherAngle);
	}

	/// <summary>
	/// Solves the quadratic formula using the inputted variables as the A, B, and C variables in the equation. 
	/// Returns two floats, one being the square root being added, and the other being the square root being subtracted.
	/// </summary>
	/// <param name="quadFormulaA"></param>
	/// <param name="quadFormulaB"></param>
	/// <param name="quadFormulaC"></param>
	/// <returns></returns>
	private static float[] SolveQuadraticFormula(float quadFormulaA, float quadFormulaB, float quadFormulaC)
	{
		//To make the formula more readable
		float quadFormulaSqrt = (float)Math.Sqrt(QuadFormulaUnderSquareRoot(quadFormulaA, quadFormulaB, quadFormulaC));
		float twoA = 2 * quadFormulaA;

		float addSqrt = (-quadFormulaB + quadFormulaSqrt) / twoA;
		float subtractSqrt = (-quadFormulaB - quadFormulaSqrt) / twoA;
		return [addSqrt, subtractSqrt];
	}

	/// <summary>
	/// Checks if the quadratic formula has any real solutions by seeing if the section underneath the square root would be equal to or above zero.
	/// </summary>
	/// <param name="quadFormulaA"></param>
	/// <param name="quadFormulaB"></param>
	/// <param name="quadFormulaC"></param>
	/// <returns></returns>
	private static bool QuadFormulaSolvable(float quadFormulaA, float quadFormulaB, float quadFormulaC)
	{
		return QuadFormulaUnderSquareRoot(quadFormulaA, quadFormulaB, quadFormulaC) >= 0;
	}

	private static float QuadFormulaUnderSquareRoot(float quadFormulaA, float quadFormulaB, float quadFormulaC)
	{
		return (float)Math.Pow(quadFormulaB, 2) - 4 * quadFormulaA * quadFormulaC;
	}

	private static float AngleDistance(float angleA, float angleB)
	{
		return MathHelper.WrapAngle(angleA - angleB);
	}

	private static float FlipAngle(float curAngle)
	{
		curAngle -= AngleDistance(curAngle, MathHelper.PiOver2) * 2;
		curAngle = MathHelper.WrapAngle(curAngle);

		return curAngle;
	}
}
