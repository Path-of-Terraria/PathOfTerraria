using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader.IO;
using TextCopy;

namespace PathOfTerraria.Core.Items;

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

			ClipboardService.SetText(StringTagRelation.FromTag(tag));
		}
#if DEBUG
		else
		{
			TagCompound tag = [];

			item.ModItem.SaveData(tag);

			item.ModItem.LoadData(StringTagRelation.FromString(ClipboardService.GetText(), tag));
		}
	}
#endif
}
