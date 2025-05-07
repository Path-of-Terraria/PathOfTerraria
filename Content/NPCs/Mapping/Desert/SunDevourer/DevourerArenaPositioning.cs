namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

/// <summary>
/// Helper class for getting a random, valid position within the Sun Devourer's arena.<br/>
/// Use <see cref="GetPosition(Func{bool})"/> to get a random offset based on <see cref="SunDevourerNPC.IdleSpot"/> (i.e. <c>GetPosition(Func) + IdleSpot</c>).
/// </summary>
internal class DevourerArenaPositioning
{
	public const int TopXStart = -722;
	public const int TopXEnd = 722;

	public const int BottomXStart = -1284;
	public const int BottomXEnd = 1284;

	public const int YTop = -460;
	public const int YBottom = 800;

	public static Vector2 GetPosition(Func<Vector2, bool> invalidFunc = null)
	{
		float x;
		float y;

		while (true)
		{
			float yFac = Main.rand.NextFloat();
			float xFac = Main.rand.NextFloat();

			bool succeeded = true;
			x = MathHelper.Lerp(MathHelper.Lerp(TopXStart, TopXEnd, xFac), MathHelper.Lerp(BottomXStart, BottomXEnd, xFac), yFac);
			y = MathHelper.Lerp(YTop, YBottom, yFac);

			if (invalidFunc is not null && invalidFunc(new Vector2(x, y)))
			{
				succeeded = false;
			}

			if (succeeded)
			{
				break;
			}
		}

		return new(x, y);
	}
}
