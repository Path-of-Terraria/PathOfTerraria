using System.Linq;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Common.Systems.Sockets;
public class SocketPlayer : ModPlayer
{
	private static bool _blockUp = false;
	private static bool _blockDown = false;
	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		Item item = inventory[slot];
		if (!GearGlobalItem.IsGearItem(item)
			|| !GearGlobalItem.UseSockets(item)
			|| inventory == Main.LocalPlayer.armor) // shift click is uneqip :/
		{
			return false;
		}

		GearGlobalItem.ShiftClick(item, Player);
		return true;
	}
	public override bool HoverSlot(Item[] inventory, int context, int slot)
	{
		Item item = inventory[slot];
		if (GearGlobalItem.IsGearItem(item) && !GearGlobalItem.UseSockets(item))
		{
			return base.HoverSlot(inventory, context, slot);
		}

		if (Main.keyState.IsKeyDown(Keys.Up))
		{
			if (!_blockUp)
			{
				_blockUp = true;

				if (GearGlobalItem.IsGearItem(item))
				{
					GearGlobalItem.NextSocket(item);
				}
			}
		}
		else
		{
			_blockUp = false;
		}

		if (Main.keyState.IsKeyDown(Keys.Down))
		{
			if (!_blockDown)
			{
				_blockDown = true;

				if (GearGlobalItem.IsGearItem(item))
				{
					GearGlobalItem.PrevSocket(item);
				}
			}
		}
		else
		{
			_blockDown = false;
		}

		return base.HoverSlot(inventory, context, slot);
	}

	public override void OnEnterWorld()
	{
		ReEquip();
	}

	public override void OnRespawn()
	{
		ReEquip();
	}

	private void ReEquip()
	{
		var allEquipedGear = Player.armor.ToList();
		allEquipedGear.Add(Player.inventory[0]);
		allEquipedGear.ForEach(a =>
		{
			if (a.active && GearGlobalItem.IsGearItem(a))
			{
				GearGlobalItem.UnequipItem(a, Player); // re-activate things when player respawns
				GearGlobalItem.EquipItem(a, Player);
			}
		});
	}
}