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
using PathOfTerraria.Common.Systems.FullMenuPausing;

namespace PathOfTerraria.Common.Looting.VirtualBagUI;

internal class VirtualBagStoragePlayer : ModPlayer
{
	/// <summary>
	/// If the local player is using the virtual bag config.
	/// </summary>
	public static bool LocalUseVirtualBag => Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag 
	                                         && IsVirtualBagActiveContext();

	public static ModKeybind BagKeybind;

	private static Hook ExitSubworldHook = null;

	public List<Item> Storage = [];
	public bool UsesVirtualBag = true;
	public bool ConfirmedExit = false;

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
				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					MenuSafeSystem.MenuOpen = true;
				}
			}
			else
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				MenuSafeSystem.MenuOpen = false;
			}
		}
	}

	public static void ExitDetour(Action orig)
	{
		VirtualBagStoragePlayer storagePlayer = Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>();
		ref bool confirmed = ref storagePlayer.ConfirmedExit;

		if (Main.netMode == NetmodeID.Server || !storagePlayer.UsesVirtualBag || !IsVirtualBagActiveContext() ||
		    confirmed || storagePlayer.Storage.Count == 0)
		{
			orig();
			return;
		}

		confirmed = true;
		UIManager.TryToggleOrRegister(VirtualBagUIState.Identifier, "Vanilla: Mouse Text", new VirtualBagUIState());
		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			MenuSafeSystem.MenuOpen = true;
		}
	}

	public override void OnEnterWorld()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			UsesVirtualBag = ModContent.GetInstance<UIConfig>().UseVirtualBag;
			ConfirmedExit = false;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModContent.GetInstance<PlayerUseSackOfHoldingHandler>().Send((byte)Main.myPlayer, UsesVirtualBag);
			}

			UIManager.TryDisable(VirtualBagUIState.Identifier);
		}

		Storage.Clear();
		MenuSafeSystem.MenuOpen = false;
	}

	private void OverrideRightClickForVirtualBag(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv,
		int context, int slot)
	{
		bool clicked = Main.mouseRight && Main.mouseRightRelease;

		if (UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled &&
		    VirtualBagAllowed(inv[slot], Main.LocalPlayer) && clicked)
		{
			Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().Storage.Add(inv[slot].Clone());
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
		       && player.GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag
		       && IsVirtualBagActiveContext();
	}

	public override void Unload()
	{
		UIManager.TryDisable(VirtualBagUIState.Identifier);
	}
}