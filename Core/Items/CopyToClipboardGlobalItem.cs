using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Items;

/// <summary>
///		Implements tag compound data copying for modded items.
/// </summary>
internal sealed class CopyToClipboardGlobalItem : GlobalItem, CopyToClipboard.IGlobal
{
	void CopyToClipboard.IGlobal.CopyToClipboard(Item item)
	{
		if (item.ModItem is null)
		{
			return;
		}

		if (!Keyboard.GetState().PressingShift())
		{
			TagCompound tag = [];

			item.ModItem.SaveData(tag);

			SetText(StringTagRelation.FromTag(tag));
		}
#if DEBUG
		else
		{
			TagCompound tag = [];

			item.ModItem.SaveData(tag);

			item.ModItem.LoadData(StringTagRelation.FromString(GetText(), tag));
		}
	}
#endif

	// We should be using the provided Re-Logic clipboard services, but they're
	// allegedly broken on Linux.
	private static void SetText(string text)
	{
		SDL2.SDL.SDL_SetClipboardText(text);
	}

	private static string GetText()
	{
		return SDL2.SDL.SDL_GetClipboardText();
	}
}
