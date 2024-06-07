using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Content.Items.Gear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Achievements;
using Terraria.UI;

namespace PathOfTerraria.Core.Systems.Sockets;
public class UnAndEquipSocketSystem : ILoadable
{
	public void Load(Mod mod)
	{
		IL_ItemSlot.LeftClick_ItemArray_int_int += LeftClick_IL; // works for inventory modifications, not armor :/ (and not autoequip)
		IL_AchievementsHelper.HandleOnEquip += HandleOnEquip_IL; // click equip
		IL_ItemSlot.ArmorSwap += ArmorSwap_IL; // works for equipping

		IL_ItemSlot.OverrideLeftClick += OverrideLeftClick_IL;
	}
	public void Unload()
	{
		IL_ItemSlot.LeftClick_ItemArray_int_int -= LeftClick_IL; // click unequip
		IL_AchievementsHelper.HandleOnEquip -= HandleOnEquip_IL; // click equip
		IL_ItemSlot.ArmorSwap += ArmorSwap_IL;

		IL_ItemSlot.OverrideLeftClick -= OverrideLeftClick_IL;
	}
	private void LeftClick_IL(ILContext il)
	{
		var c = new ILCursor(il);
		for (int i = 0; i < c.Body.Instructions.Count; i++)
		{
			Instruction ins = c.Body.Instructions[i];
			if (ins.OpCode == OpCodes.Call && ins.Previous.OpCode == OpCodes.Ldsflda && ins.Previous.Previous.OpCode == OpCodes.Ldelema)
			{
				if (ins.Operand.ToString() == "System.Void Terraria.Utils::Swap<Terraria.Item>(T&,T&)")
				{
					c.Index = i - 4;
					c.Emit(OpCodes.Ldarg, 0);
					c.Emit(OpCodes.Ldarg, 2);
					c.Emit(OpCodes.Ldelem_Ref);
					c.EmitDelegate(PotentialGearEquip1);
					i += 4;
				}
			}
		}

		MonoModHooks.DumpIL(ModContent.GetInstance<PathOfTerraria>(), il);
	}
	public static void PotentialGearEquip1(Item onSlot)
	{
		Player player = Main.player[Main.myPlayer];
		Item inHand = Main.mouseItem;

		if (onSlot.active && onSlot.ModItem is Gear)
		{
			if ((onSlot.ModItem as Gear).IsThisItemActive(player))
			{
				(onSlot.ModItem as Gear).UnEquipItem(player);
			}
		}
	}
	private void HandleOnEquip_IL(ILContext il)
	{
		var c = new ILCursor(il);
		c.Index = c.Body.CodeSize;
		c.Emit(OpCodes.Ldarg, 0);
		c.Emit(OpCodes.Ldarg, 1);
		c.EmitDelegate(PotentialGearEquip2);
	}
	public static void PotentialGearEquip2(Player player, Item item)
	{
		if (item.active && item.ModItem is Gear)
		{
			if ((item.ModItem as Gear).IsThisItemActive(player))
			{
				(item.ModItem as Gear).EquipItem(player);
			}
		}
	}

	private void ArmorSwap_IL(ILContext il)
	{
		var c = new ILCursor(il);
		c.Index = c.Body.CodeSize - 1;
		c.Emit(OpCodes.Ldarg, 0);
		c.Emit(OpCodes.Ldloc, 2);
		c.EmitDelegate(PotentialGearEquip3);
	}
	public static void PotentialGearEquip3(Item itemEquip, Item itemUnEquip)
	{
		Player player = Main.player[Main.myPlayer];
		if (itemEquip.active && itemEquip.ModItem is Gear)
		{
			(itemEquip.ModItem as Gear).EquipItem(player);
		}

		if (itemUnEquip.active && itemUnEquip.ModItem is Gear)
		{
			(itemUnEquip.ModItem as Gear).UnEquipItem(player);
		}
	}

	public void OverrideLeftClick_IL(ILContext il)
	{
		var c = new ILCursor(il);
		int occurance = 0;
		for (int i = 0; i < c.Body.Instructions.Count; i++)
		{
			Instruction ins = c.Body.Instructions[i];
			if (ins.OpCode == OpCodes.Callvirt)
			{
				if (ins.Operand.ToString() == "Terraria.Item Terraria.Player::GetItem(System.Int32,Terraria.Item,Terraria.GetItemSettings)")
				{
					occurance++;
					if (occurance == 2)
					{
						c.Index = i;
						break;
					}
				}
			}
		}

		c.Remove();
		c.EmitDelegate(GetItemReplace);
		c.Index -= 3;
		c.Remove(); // stop it from indexing inventory automatically
		c.Index -= 6;
		c.RemoveRange(3); // remove the class to call it on by default, as we do that below.
	}
	public static Item GetItemReplace(int plr, Item[] inv, int slot, GetItemSettings settings)
	{
		Player player = Main.player[plr];
		Item moveItem = inv[slot];
		Item nItem = player.GetItem(plr, moveItem, settings);

		if (inv == player.armor && moveItem.ModItem is Gear)
		{
			(moveItem.ModItem as Gear).UnEquipItem(player);
		}

		return nItem;
	}
}