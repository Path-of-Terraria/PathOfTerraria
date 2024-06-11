using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Helpers;
using Terraria.GameInput;

namespace PathOfTerraria.Core.Systems.ModPlayers;

internal class WeaponSwapPlayer : ModPlayer
{
	internal static ModKeybind WeaponSwapKeybind { get; private set; }

	public override void Load()
	{
		WeaponSwapKeybind = KeybindLoader.RegisterKeybind(Mod, "WeaponSwap", Keys.Z);
	}

	public override void Unload()
	{
		WeaponSwapKeybind = null;
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (!WeaponSwapKeybind.JustPressed)
		{
			return;
		}

		int slotToSwapTo = 10; // Slot immediately under the weapon slot
		if (!Player.inventory[slotToSwapTo].IsWeapon())
		{
			// Fallback to first weapon we find in inventory
			for (int i = 2; i < Main.InventoryItemSlotsCount; i++)
			{
				Item item = Player.inventory[i];
				if (item.IsWeapon())
				{
					slotToSwapTo = i;
					break;
				}
				
				if (i == 49)
				{
					return;
				}
			}
		}

		(Player.inventory[0], Player.inventory[slotToSwapTo]) = (Player.inventory[slotToSwapTo], Player.inventory[0]);
	}
}