using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.GameInput;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveTreeHotkey : ModPlayer
{
	private static ModKeybind _toggleStatUIKey;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		_toggleStatUIKey = KeybindLoader.RegisterKeybind(Mod, "PassiveTreeUIKey", Keys.P);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (!_toggleStatUIKey.JustPressed)
		{
			return;
		}

		Main.playerInventory = true;
		SmartUiLoader.GetUiState<TreeState>().Toggle();
	}
}
