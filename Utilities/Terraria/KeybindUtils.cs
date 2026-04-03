using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Terraria.GameInput;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

internal static class KeybindUtils
{
    public static string Name(this ModKeybind keybind)
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Name")]
        static extern string Getter(ModKeybind kb);
        return Getter(keybind);
    }
    public static string FullName(this ModKeybind keybind)
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_FullName")]
        static extern string Getter(ModKeybind kb);
        return Getter(keybind);
    }

    /// <summary> Safer variant of <see cref="ModKeybind.GetAssignedKeys"/>. Use this to prevent issues with -skipselect. </summary>
    public static bool TryGetAssignedKeys(this ModKeybind keybind, [NotNullWhen(true)] out List<string>? result, InputMode mode = InputMode.Keyboard)
	{
		if (PlayerInput.CurrentProfile.InputModes[mode].KeyStatus.ContainsKey(keybind.FullName()))
		{
			result = keybind.GetAssignedKeys(mode);
			return true;
		}

		result = null;
		return false;
	}
}
