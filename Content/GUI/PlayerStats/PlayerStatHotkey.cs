using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Core.Loaders.UILoading;
using Terraria.GameInput;

namespace PathOfTerraria.Content.GUI.PlayerStats;

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
			UILoader.GetUIState<PlayerStatUIState>().Toggle();
		}
	}
}
