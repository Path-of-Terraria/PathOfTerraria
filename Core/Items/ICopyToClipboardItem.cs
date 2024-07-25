using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;
using Terraria.ModLoader.IO;
using Terraria.UI;
using TextCopy;

namespace PathOfTerraria.Core.Items;

public interface ICopyToClipboardItem
{
	private sealed class CopyToClipboardItemImpl : ILoadable
	{
		private ModKeybind _copyItemKeybind;

		void ILoadable.Load(Mod mod)
		{
			_copyItemKeybind = KeybindLoader.RegisterKeybind(mod, "CopyItemInfo", "I");
			On_ItemSlot.LeftClick_ItemArray_int_int += AddKeybindPressEvent;
		}

		void ILoadable.Unload()
		{
			_copyItemKeybind = null;
		}

		private void AddKeybindPressEvent(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			Item item = inv[slot];

			if (_copyItemKeybind.JustPressed && item.active && item.TryGetInterface(out ICopyToClipboardItem copyToClipboardItem))
			{
				copyToClipboardItem.CopyToClipboard(item);
			}

			orig(inv, context, slot);
		}
	}

	void CopyToClipboard(Item item)
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
}
