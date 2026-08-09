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
	public Item SlottedItem = new(0);

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

	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		if (inventory != Player.inventory
			|| context != ItemSlot.Context.InventoryItem
			|| slot < 0
			|| slot >= inventory.Length)
		{
			return false;
		}

		Item item = inventory[slot];

		if (!CurrencyPouchAllowed(item))
		{
			return false;
		}

		if (!TryStoreItem(item, out int stored))
		{
			return false;
		}

		SoundEngine.PlaySound(SoundID.Grab);

		if (UIManager.TryGet(CurrencyPouchUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
		{
			(data.Value as CurrencyPouchUIState).Toggle();
			(data.Value as CurrencyPouchUIState).Toggle();
		}

		return true;
	}

	public static bool CurrencyPouchAllowed(Item item)
	{
		return item.ModItem is CurrencyShard;
	}

	public bool TryStoreItem(Item item, out int stored)
	{
		stored = 0;

		if (!CurrencyPouchAllowed(item) || item.IsAir)
		{
			return false;
		}

		StorageByType.TryAdd(item.type, 0);
		int storageSpace = item.maxStack - StorageByType[item.type];

		if (storageSpace <= 0)
		{
			return false;
		}

		stored = Math.Min(item.stack, storageSpace);
		StorageByType[item.type] += stored;
		item.stack -= stored;

		if (item.stack <= 0)
		{
			item.TurnToAir();
		}

		return stored > 0;
	}

	public override void Unload()
	{
		UIManager.TryDisable(CurrencyPouchUIState.Identifier);
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("keys", (int[])[.. StorageByType.Keys]);
		tag.Add("values", (int[])[.. StorageByType.Values]);
		tag.Add("slottedItem", SlottedItem);
	}

	public override void LoadData(TagCompound tag)
	{
		int[] keys = tag.GetIntArray("keys");
		int[] values = tag.GetIntArray("values");

		SlottedItem = tag.Get<Item>("slottedItem");

		Debug.Assert(keys.Length == values.Length, "[CurrencyPouchStoragePlayer] Key-value pairs don't match in length.");

		StorageByType.Clear();

		for (int i = 0; i < keys.Length; ++i)
		{
			StorageByType.Add(keys[i], values[i]);
		}
	}
}
