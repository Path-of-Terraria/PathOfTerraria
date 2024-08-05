using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Core.Items;
using Terraria.GameContent.Achievements;
using Terraria.UI;

namespace PathOfTerraria.Common.Systems.Sockets;
public class UnAndEquipSocketSystem : ILoadable
{
	public void Load(Mod mod)
	{
		IL_ItemSlot.LeftClick_ItemArray_int_int += LeftClick_IL; // works for inventory modifications, not armor :/ (and not autoequip)
		IL_AchievementsHelper.HandleOnEquip += HandleOnEquip_IL; // click equip
		IL_ItemSlot.ArmorSwap += ArmorSwap_IL; // works for equipping

		IL_ItemSlot.OverrideLeftClick += OverrideLeftClick_IL;
	}

	public void Unload() { }

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
	}
	
	public static void PotentialGearEquip1(Item onSlot)
	{
		Player player = Main.player[Main.myPlayer];
		Item inHand = Main.mouseItem;

		if (onSlot.active && GearGlobalItem.IsGearItem(onSlot))
		{
			if (GearGlobalItem.IsActive(player, onSlot))
			{
				GearGlobalItem.UnequipItem(onSlot, player);
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
		if (item.active && GearGlobalItem.IsGearItem(item))
		{
			if (GearGlobalItem.IsActive(player, item))
			{
				GearGlobalItem.EquipItem(item, player);
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
		if (itemEquip.active && GearGlobalItem.IsGearItem(itemEquip))
		{
			GearGlobalItem.EquipItem(itemEquip, player);
		}

		if (itemUnEquip.active && GearGlobalItem.IsGearItem(itemUnEquip))
		{
			GearGlobalItem.UnequipItem(itemUnEquip, player);
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

		if (inv == player.armor && GearGlobalItem.IsGearItem(moveItem))
		{
			GearGlobalItem.UnequipItem(moveItem, player);
		}

		return nItem;
	}
}