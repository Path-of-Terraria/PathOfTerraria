namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

/// <summary>
/// Helper class for getting a random, valid position within the Sun Devourer's arena.<br/>
/// Use <see cref="GetPosition(Func{bool})"/> to get a random offset based on <see cref="SunDevourerNPC.IdleSpot"/> (i.e. <c>GetPosition(Func) + IdleSpot</c>).
/// </summary>
internal class DevourerArenaPositioning
{
	public const int TopXStart = -692;
	public const int TopXEnd = 692;

	public const int BottomXStart = -1284;
	public const int BottomXEnd = 1284;

	public const int YTop = -460;
	public const int YBottom = 1070;

	public delegate bool InvalidatePositionDelegate(Vector2 input, out Vector2 output);

	public static Vector2 GetPosition(float xFac, float yFac, out bool couldntPlace, InvalidatePositionDelegate invalidFunc = null)
	{
		float x;
		float y;
		int repeats = 0;

		while (true)
		{
			if (repeats > 30000)
			{
				couldntPlace = true;
				return Vector2.Zero;
			}

			bool succeeded = true;
			x = MathHelper.Lerp(MathHelper.Lerp(TopXStart, TopXEnd, xFac), MathHelper.Lerp(BottomXStart, BottomXEnd, xFac), yFac);
			y = MathHelper.Lerp(YTop, YBottom, yFac);

			if (invalidFunc is not null && invalidFunc(new Vector2(x, y), out Vector2 output))
			{
				succeeded = false;

				xFac = output.X;
				yFac = output.Y;
			}

			if (succeeded)
			{
				break;
			}

			repeats++;
		}

		couldntPlace = false;
		return new(x, y);
	}

	public static Vector2 GetRandomPosition(out bool couldntPlace, InvalidatePositionDelegate invalidFunc = null)
	{
		float xFac = Main.rand.NextFloat();
		float yFac = Main.rand.NextFloat();

		return GetPosition(xFac, yFac, out couldntPlace, invalidFunc);
	}
}
