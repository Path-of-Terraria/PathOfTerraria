using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Core.UI;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchStoragePlayer : ModPlayer 
{
	public static ModKeybind CurrencyPouchKeybind;

	public Dictionary<int, int> StorageByType = [];
	public Item SlottedItem = new(0);

	/// <summary>
	/// Legacy id-keyed storage that couldn't be resolved this session, kept verbatim so it survives a re-save.
	/// See <see cref="LoadLegacyStorage"/>.
	/// </summary>
	private readonly Dictionary<int, int> _unresolvedLegacyStorage = [];

	/// <summary>
	/// Original id-keyed data retained after a successful migration. Legacy ids cannot prove which shard they used to
	/// identify, so keeping this separate backup makes a best-effort migration reversible without duplicating storage.
	/// </summary>
	private readonly Dictionary<int, int> _legacyStorageBackup = [];

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

	public override void OnEnterWorld()
	{
		if (Main.myPlayer == Player.whoAmI && _unresolvedLegacyStorage.Count > 0)
		{
			Main.NewText(Language.GetTextValue("Mods.PathOfTerraria.UI.CurrencyPouch.LegacyRecoveryWarning"), Color.OrangeRed);
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
		// Item ids are handed out at load time and shift whenever the enabled mod set changes, so they can't be
		// persisted directly - storage is keyed by the shard's permanent full name instead.
		List<string> names = [];
		List<int> counts = [];

		foreach ((int type, int count) in StorageByType)
		{
			if (count <= 0 || !TryGetShardName(type, out string name))
			{
				continue;
			}

			names.Add(name);
			counts.Add(count);
		}

		if (names.Count > 0)
		{
			tag.Add("shardNames", names);
			tag.Add("shardCounts", counts);
		}

		if (_unresolvedLegacyStorage.Count > 0)
		{
			tag.Add("keys", (int[])[.. _unresolvedLegacyStorage.Keys]);
			tag.Add("values", (int[])[.. _unresolvedLegacyStorage.Values]);
		}

		if (_legacyStorageBackup.Count > 0)
		{
			tag.Add("legacyBackupKeys", (int[])[.. _legacyStorageBackup.Keys]);
			tag.Add("legacyBackupValues", (int[])[.. _legacyStorageBackup.Values]);
		}

		tag.Add("slottedItem", SlottedItem);
	}

	public override void LoadData(TagCompound tag)
	{
		StorageByType.Clear();
		_unresolvedLegacyStorage.Clear();
		_legacyStorageBackup.Clear();

		if (tag.ContainsKey("slottedItem"))
		{
			SlottedItem = tag.Get<Item>("slottedItem");
		}

		SlottedItem ??= new Item(0);

		IList<string> names = tag.GetList<string>("shardNames");
		IList<int> counts = tag.GetList<int>("shardCounts");

		Debug.Assert(names.Count == counts.Count, "[CurrencyPouchStoragePlayer] Name-count pairs don't match in length.");

		for (int i = 0; i < Math.Min(names.Count, counts.Count); ++i)
		{
			if (ModContent.TryFind(names[i], out ModItem shard) && shard is CurrencyShard)
			{
				AddToStorage(shard.Type, counts[i]);
			}
		}

		LoadRawStorage(tag, "legacyBackupKeys", "legacyBackupValues", _legacyStorageBackup);
		LoadLegacyStorage(tag);
	}

	/// <summary>
	/// Loads pouch data written before storage was keyed by name. That data holds raw item ids, which only mean
	/// anything for the exact mod and content layout that saved them. Enabling or disabling a mod that loads before
	/// this one shifts every id uniformly, while switching Path of Terraria versions can also reorder ids internally.
	/// Directly valid ids are migrated with their original data retained as a backup. Otherwise, a uniform shift is
	/// applied only when exactly one shift maps every saved id to a currency shard. Ambiguous data is kept verbatim so
	/// restoring the old mod set and Path of Terraria version can still recover it.
	/// </summary>
	private void LoadLegacyStorage(TagCompound tag)
	{
		int[] keys = tag.GetIntArray("keys");
		int[] values = tag.GetIntArray("values");

		Debug.Assert(keys.Length == values.Length, "[CurrencyPouchStoragePlayer] Key-value pairs don't match in length.");

		int count = Math.Min(keys.Length, values.Length);

		if (count == 0)
		{
			return;
		}

		int offset = 0;
		bool canMigrate = LegacyIdsMatchOffset(keys, count, offset) || TryFindUniqueLegacyOffset(keys, count, out offset);

		for (int i = 0; i < count; ++i)
		{
			if (canMigrate)
			{
				AddToStorage(keys[i] + offset, values[i]);
				_legacyStorageBackup[keys[i]] = values[i];
			}
			else
			{
				_unresolvedLegacyStorage[keys[i]] = values[i];
			}
		}
	}

	private static bool TryFindUniqueLegacyOffset(int[] keys, int count, out int offset)
	{
		offset = 0;
		bool found = false;

		foreach (CurrencyShard shard in ModContent.GetContent<CurrencyShard>())
		{
			long candidateValue = (long)shard.Type - keys[0];

			if (candidateValue is < int.MinValue or > int.MaxValue)
			{
				continue;
			}

			int candidate = (int)candidateValue;

			if (!LegacyIdsMatchOffset(keys, count, candidate))
			{
				continue;
			}

			if (found)
			{
				return false;
			}

			found = true;
			offset = candidate;
		}

		return found;
	}

	private static bool LegacyIdsMatchOffset(int[] keys, int count, int offset)
	{
		for (int i = 0; i < count; ++i)
		{
			long mappedType = (long)keys[i] + offset;

			if (mappedType is <= ItemID.None or >= int.MaxValue || !TryGetShardName((int)mappedType, out _))
			{
				return false;
			}
		}

		return true;
	}

	private static void LoadRawStorage(TagCompound tag, string keysName, string valuesName, Dictionary<int, int> storage)
	{
		int[] keys = tag.GetIntArray(keysName);
		int[] values = tag.GetIntArray(valuesName);
		int count = Math.Min(keys.Length, values.Length);

		Debug.Assert(keys.Length == values.Length, $"[CurrencyPouchStoragePlayer] {keysName}-{valuesName} pairs don't match in length.");

		for (int i = 0; i < count; ++i)
		{
			storage[keys[i]] = values[i];
		}
	}

	private void AddToStorage(int type, int count)
	{
		if (count <= 0)
		{
			return;
		}

		StorageByType.TryAdd(type, 0);
		StorageByType[type] = Math.Min(StorageByType[type] + count, ContentSamples.ItemsByType[type].maxStack);
	}

	private static bool TryGetShardName(int type, out string name)
	{
		name = null;

		if (ItemLoader.GetItem(type) is not CurrencyShard shard)
		{
			return false;
		}

		name = shard.FullName;
		return true;
	}
}
