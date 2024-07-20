using Microsoft.Xna.Framework.Input;

namespace PathOfTerraria.Core.Systems.ModPlayers;

public sealed class GearSwapKeybind : ModSystem
{
	public static ModKeybind? SwapKeybind { get; private set; }

	public override void Load()
	{
		SwapKeybind = KeybindLoader.RegisterKeybind(Mod, $"{nameof(PathOfTerraria)}:{nameof(GearSwapKeybind)}", Keys.Z);
	}

	public override void Unload()
	{
		SwapKeybind = null;
	}
}