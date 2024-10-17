using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.GameInput;

namespace PathOfTerraria.Common.UI.PlayerStats;

internal class PlayerStatHotkey : ModPlayer
{
	public static ModKeybind ToggleStatUIKey = null;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		ToggleStatUIKey = KeybindLoader.RegisterKeybind(Mod, "StatUIKey", Keys.V);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ToggleStatUIKey.JustPressed)
		{
			SmartUiLoader.GetUiState<PlayerStatUIState>().Toggle();
		}
	}
}
