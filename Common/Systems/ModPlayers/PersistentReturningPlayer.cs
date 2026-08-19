using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using SubworldLibrary;
using Terraria.ID;

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

			// SubworldLibrary resets the server's section state for a returning client, and this
			// teleport only happens clientside, so the server is still streaming sections around
			// wherever it last saw us. Mirrors what BossDomainLivesPlayer.ExitDomain does after its
			// respawn.
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				RequestCheckSectionHandler.Send(Player.Center);
			}
		}
	}
}
