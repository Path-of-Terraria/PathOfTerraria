using System.Linq;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Common.Systems.Sockets;
public class SocketPlayer : ModPlayer
{
	private static bool _blockUp = false;
	private static bool _blockDown = false;
	public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		Item item = inventory[slot];
		if (item.ModItem is not Gear
			|| item.ModItem is Gear && !(item.ModItem as Gear).UseSockets()
			|| inventory == Main.LocalPlayer.armor) // shift click is uneqip :/
		{
			return false;
		}

		var gear = item.ModItem as Gear;

		gear.ShiftClick(Player);

		return true;
	}
	public override bool HoverSlot(Item[] inventory, int context, int slot)
	{
		Item item = inventory[slot];
		if (item.ModItem is Gear && !(item.ModItem as Gear).UseSockets())
		{
			return base.HoverSlot(inventory, context, slot);
		}

		if (Main.keyState.IsKeyDown(Keys.Up))
		{
			if (!_blockUp)
			{
				_blockUp = true;

				if (item.ModItem is Gear)
				{
					(item.ModItem as Gear).NextSocket();
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

				if (item.ModItem is Gear)
				{
					(item.ModItem as Gear).PrevSocket();
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
			if (a.active && a.ModItem is Gear)
			{
				(a.ModItem as Gear).UnEquipItem(Player); // re-activate things when player respawns
				(a.ModItem as Gear).EquipItem(Player);
			}
		});
	}
}