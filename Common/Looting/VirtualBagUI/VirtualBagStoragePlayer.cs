using MonoMod.RuntimeDetour;
using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;
using PathOfTerraria.Common.Subworlds;

namespace PathOfTerraria.Common.Looting.VirtualBagUI;

internal class VirtualBagStoragePlayer : ModPlayer
{
	/// <summary>
	/// If the local player is using the virtual bag config.
	/// </summary>
	public static bool LocalUseVirtualBag => Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag &&
	                                         IsVirtualBagActiveContext();

	public static ModKeybind BagKeybind;

	private static Hook ExitSubworldHook = null;

	public List<Item> MatchedStorage = [];
	public List<Item> FilteredOutStorage = [];
	/// <summary>
	///		Name of the filter that was active when items most recently entered storage. Empty when no
	///		filter has been applied this session. Shown in the Sack of Holding header.
	/// </summary>
	public string LastFilterName = string.Empty;

	public bool UsesVirtualBag = true;
	public bool ConfirmedExit = false;
	public Dictionary<int, int> ConfluxResourcesCollected = [];
	public Dictionary<int, int> CurrencyShardsCollected = [];

	public bool HasAnyStorage => MatchedStorage.Count > 0 || FilteredOutStorage.Count > 0;

	private static bool IsVirtualBagActiveContext()
	{
		return SubworldSystem.Current is MappingWorld and not RavencrestSubworld;
	}

	public override void Load()
	{
		On_ItemSlot.RightClick_ItemArray_int_int += OverrideRightClickForVirtualBag;

		ExitSubworldHook = new Hook(typeof(SubworldSystem).GetMethod(nameof(SubworldSystem.Exit)), ExitDetour);

		if (!Main.dedServ)
		{
			BagKeybind = KeybindLoader.RegisterKeybind(Mod, "VirtualBagHotkey", "O");
		}
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (BagKeybind.JustPressed && VirtualBagItemFunctionality.IsInAPlaceForPickup && LocalUseVirtualBag)
		{
			UIManager.TryToggleOrRegister(VirtualBagUIState.Identifier, "Vanilla: Mouse Text", new VirtualBagUIState());

			if (UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled)
			{
				SoundEngine.PlaySound(SoundID.MenuOpen);
			}
			else
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
		}
	}

	public static void ExitDetour(Action orig)
	{
		VirtualBagStoragePlayer storagePlayer = Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>();
		ref bool confirmed = ref storagePlayer.ConfirmedExit;

		if (Main.netMode == NetmodeID.Server || !storagePlayer.UsesVirtualBag || !IsVirtualBagActiveContext() ||
		    confirmed || !storagePlayer.HasAnyStorage)
		{
			orig();
			return;
		}

		confirmed = true;
		UIManager.TryToggleOrRegister(VirtualBagUIState.Identifier, "Vanilla: Mouse Text", new VirtualBagUIState());
	}

	public override void OnEnterWorld()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			UsesVirtualBag = ModContent.GetInstance<UIConfig>().UseVirtualBag;
			ConfirmedExit = false;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				PlayerUseSackOfHoldingHandler.Send(UsesVirtualBag);
			}

			UIManager.TryDisable(VirtualBagUIState.Identifier);
		}

		ConfluxResourcesCollected.Clear();
		CurrencyShardsCollected.Clear();
		MatchedStorage.Clear();
		FilteredOutStorage.Clear();
		LastFilterName = string.Empty;
	}

	public void RecordConfluxResource(int itemType, int amount)
	{
		RecordSessionLoot(ConfluxResourcesCollected, itemType, amount);
	}

	public void RecordCurrencyShard(int itemType, int amount)
	{
		RecordSessionLoot(CurrencyShardsCollected, itemType, amount);
	}

	private static void RecordSessionLoot(Dictionary<int, int> storage, int itemType, int amount)
	{
		if (itemType <= ItemID.None || amount <= 0)
		{
			return;
		}

		storage.TryAdd(itemType, 0);
		storage[itemType] += amount;
	}

	private void OverrideRightClickForVirtualBag(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv,
		int context, int slot)
	{
		bool clicked = Main.mouseRight && Main.mouseRightRelease;

		if (UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled &&
		    VirtualBagAllowed(inv[slot], Main.LocalPlayer) && clicked)
		{
			Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().MatchedStorage.Add(inv[slot].Clone());
			(data.Value as VirtualBagUIState).RefreshStorage();
			inv[slot].TurnToAir();

			SoundEngine.PlaySound(SoundID.Grab);
			return;
		}

		orig(inv, context, slot);
	}

	public static bool VirtualBagAllowed(Item item, Player player)
	{
		return (!item.IsACoin && (item.ModItem is Gear || item.TryGetGlobalItem(out PoTGlobalItem _)))
		       && item.ammo == 0 && !ItemID.Sets.IsAMaterial[item.type]
		       && player.GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag
		       && IsVirtualBagActiveContext();
	}

	public override void Unload()
	{
		UIManager.TryDisable(VirtualBagUIState.Identifier);
	}
}
