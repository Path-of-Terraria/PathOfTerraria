using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class PersistentReturningPlayer : ModPlayer
{
	public Vector2 ReturnPosition = Vector2.Zero;
	public bool CheckInMainWorld = true;

	public override void OnEnterWorld()
	{
		if (ReturnPosition != Vector2.Zero && CheckInMainWorld && SubworldSystem.Current is null)
		{
			Player.Center = ReturnPosition;
			Player.fallStart = (int)Player.Center.Y / 16;
			ReturnPosition = Vector2.Zero;
		}
	}
}
