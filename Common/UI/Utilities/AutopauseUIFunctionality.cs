using PathOfTerraria.Core.UI;
using Terraria.ID;

namespace PathOfTerraria.Common.UI.Utilities;

public class AutopauseUIFunctionality : ModSystem
{
	public override void Load()
	{
		On_Main.CanPauseGame += HijackPauseGame;
	}

	private static bool HijackPauseGame(On_Main.orig_CanPauseGame orig)
	{
		bool vanilla = orig();

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			return vanilla;
		}

		bool autopause = false;

		foreach (UIManager.UIStateData data in UIManager.Data)
		{
			if (data.UserInterface?.CurrentState is IAutopauseUI state && data.Enabled)
			{
				autopause = true;
				break;
			}
		}

		return vanilla || autopause;
	}
}
