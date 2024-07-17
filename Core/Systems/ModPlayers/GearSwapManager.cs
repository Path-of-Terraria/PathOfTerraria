using PathOfTerraria.Core.Systems.ModPlayers;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Players;

// TODO: Implement the functionality of having specific sets, so you cant have specific combos.
public sealed class GearSwapManager : ModPlayer
{
	/// <summary>
	///		The player's gear inventory, reserved for a weapon and an offhand accessory.
	/// </summary>
	public Item?[] Inventory = new[] { new Item(ItemID.None), new Item(ItemID.None) };
	
	public override void UpdateEquips()
	{
		if (!GearSwapKeybind.SwapKeybind.JustPressed)
		{
			return;
		}

		var previousWeapon = Player.inventory[0];
		var previousOffhand = Player.armor[6];

		Player.inventory[0] = Inventory[0];
		Player.armor[6] = Inventory[1];

		Inventory[0] = previousWeapon;
		Inventory[1] = previousOffhand;
	}
}