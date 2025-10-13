using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Core.UI;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchStoragePlayer : ModPlayer 
{
	public static ModKeybind CurrencyPouchKeybind;

	public Dictionary<int, int> StorageByType = [];

	public override void Load()
	{
		if (!Main.dedServ)
		{
			CurrencyPouchKeybind = KeybindLoader.RegisterKeybind(Mod, "CurrencyPouchHotkey", "P");
		}
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (CurrencyPouchKeybind.JustPressed)
		{
			UIManager.TryToggleOrRegister(CurrencyPouchUIState.Identifier, "Vanilla: Mouse Text", new CurrencyPouchUIState(), 0, InterfaceScaleType.UI);

			if (UIManager.TryGet(CurrencyPouchUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
			{
				SoundEngine.PlaySound(SoundID.MenuOpen);
			}
			else
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
		}
	}

	public static bool CurrencyPouchAllowed(Item item)
	{
		return item.ModItem is CurrencyShard;
	}

	public override void Unload()
	{
		UIManager.TryDisable(CurrencyPouchUIState.Identifier);
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("keys", (int[])[.. StorageByType.Keys]);
		tag.Add("values", (int[])[.. StorageByType.Values]);
	}

	public override void LoadData(TagCompound tag)
	{
		int[] keys = tag.GetIntArray("keys");
		int[] values = tag.GetIntArray("values");

		Debug.Assert(keys.Length == values.Length, "[CurrencyPouchStoragePlayer] Key-value pairs don't match in length.");

		StorageByType.Clear();

		for (int i = 0; i < keys.Length; ++i)
		{
			StorageByType.Add(keys[i], values[i]);
		}
	}
}
