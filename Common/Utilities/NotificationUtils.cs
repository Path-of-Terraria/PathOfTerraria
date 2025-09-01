using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Utilities;

public static class NotificationUtils
{
	public static void ShowNotification(string key, Color color, params object[] parameters)
	{
		switch (Main.netMode)
		{
			case NetmodeID.SinglePlayer:
				Main.NewText(Language.GetTextValue(key, parameters), color);
				break;
			case NetmodeID.MultiplayerClient:
			case NetmodeID.Server:
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key, parameters), color);
				break;
		}
	}
}