using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Socketables;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Items;

internal sealed partial class GearGlobalItem : GlobalItem, InsertAdditionalTooltipLines.IGlobal, ExtraRolls.IGlobal, SwapItemModifiers.IGlobal
{
	private static HashSet<int> _optInGearItems = [];

	public void EquipItem(Player player, Item item)
	{
		if (!UseSockets())
		{
			return;
		}

		foreach (Socketable socket in Sockets)
		{
			socket.OnSocket(player, item);
		}
	}

	public void UnequipItem(Player player, Item item)
	{
		if (!UseSockets())
		{
			return;
		}

		foreach (Socketable socket in Sockets)
		{
			socket.OnUnsocket(player, item);
		}
	}

	public bool UseSockets()
	{
		return Sockets.Length > 0;
	}

	public void NextSocket()
	{
		SelectedSocket++;
		if (SelectedSocket >= Sockets.Length)
		{
			SelectedSocket = 0;
		}
	}

	public void PrevSocket()
	{
		SelectedSocket--;
		if (SelectedSocket < 0)
		{
			SelectedSocket = Sockets.Length - 1;
		}
	}

	public void Socket(Player player, Socketable socket)
	{
		if (Sockets.Length == 0)
		{
			return;
		}

		if (Sockets[SelectedSocket] is not null)
		{
			Sockets[SelectedSocket].OnUnsocket(player, socket.Item);
			Main.mouseItem = Sockets[SelectedSocket].Item;
		}
		else
		{
			Main.mouseItem = new Item();
		}

		Sockets[SelectedSocket] = socket;

		if (IsActive(player, socket.Item))
		{
			socket.OnSocket(player, socket.Item);
		}
	}

	public void ShiftClick(Player player, Item item)
	{
		if (Main.mouseItem.active && Main.mouseItem.ModItem is Socketable)
		{
			Socket(player, Main.mouseItem.ModItem as Socketable);
		}
		else if (Sockets[SelectedSocket] is not null)
		{
			if (IsActive(player, item))
			{
				Sockets[SelectedSocket].OnUnsocket(player, item);
			}

			Main.mouseItem = Sockets[SelectedSocket].Item;
			Sockets[SelectedSocket] = null;
		}
	}

	public override void Unload()
	{
		base.Unload();

		_optInGearItems = null;
	}

	public override void OnCreated(Item item, ItemCreationContext context)
	{
		base.OnCreated(item, context);

		if (!IsGearItem(item))
		{
			return;
		}

		if (context is not RecipeItemCreationContext)
		{
			return;
		}

		PoTInstanceItemData data = item.GetInstanceData();
		data.Rarity = ItemRarity.Magic;
		data.Affixes.Clear();
		PoTItemHelper.Roll(item, PoTItemHelper.PickItemLevel());
	}

	public static bool IsActive(Player player, Item item)
	{
		return player.inventory[0] == item || player.armor.Contains(item);
	}

	public static void MarkItemAsGear(int type)
	{
		_optInGearItems.Add(type);
	}

	public static bool IsGearItem(Item item)
	{
		return item.ModItem is Gear || _optInGearItems.Contains(item.type);
	}

	void InsertAdditionalTooltipLines.IGlobal.InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier)
	{
		if (!IsGearItem(item))
		{
			return;
		}

		GearInstanceData gearData = item.GetGearData();

		if (gearData.Sockets.Length > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "Space", " "));
		}

		for (int i = 0; i < gearData.Sockets.Length; i++)
		{
			Socketable socket = gearData.Sockets[i];
			string text = "";

			if (socket is not null)
			{
				text = GenerateName.Invoke(socket.Item);
			}

			var affixLine = new TooltipLine(Mod, $"Socket{i}",
				$"[i:{(i == gearData.SelectedSocket ? ItemID.NanoBullet : ItemID.ChlorophyteBullet)}] " + text);
			tooltips.Add(affixLine);
		}
	}

	void ExtraRolls.IGlobal.ExtraRolls(Item item)
	{
		if (!IsGearItem(item))
		{
			return;
		}

		GearInstanceData gearData = item.GetGearData();
		gearData.SelectedSocket = 0;

		PoTInstanceItemData data = item.GetInstanceData();

		int maxSockets = data.Rarity switch // what to do if we roll less sockets than what we have equipped?
		{                                   // maby just not allow to roll if we have any sockets?
			ItemRarity.Normal => Main.rand.Next(2),
			ItemRarity.Magic => Main.rand.Next(1, 2),
			ItemRarity.Rare => Main.rand.Next(1, 3),
			_ => 0,
		};
		gearData.Sockets = new Socketable[maxSockets];
	}

	public override void UpdateEquip(Item item, Player player)
	{
		base.UpdateEquip(item, player);

		if (!IsGearItem(item))
		{
			return;
		}

		GearInstanceData gearData = item.GetGearData();

		foreach (Socketable socket in gearData.Sockets.Where(s => s is not null))
		{
			socket.UpdateEquip(player, item);
		}
	}

	void SwapItemModifiers.IGlobal.SwapItemModifiers(Item item, EntityModifier swapItemModifier)
	{
		if (!IsGearItem(item))
		{
			return;
		}

		if (item.headSlot >= 0 && Main.LocalPlayer.armor[0].active && Main.LocalPlayer.armor[0].ModItem is Gear headGear)
		{
			PoTItemHelper.ApplyAffixes(headGear.Item, swapItemModifier);
		}
		else if (item.bodySlot >= 0 && Main.LocalPlayer.armor[1].active && Main.LocalPlayer.armor[0].ModItem is Gear bodyGear)
		{
			PoTItemHelper.ApplyAffixes(bodyGear.Item, swapItemModifier);
		}
		else if (item.legSlot >= 0 && Main.LocalPlayer.armor[2].active && Main.LocalPlayer.armor[0].ModItem is Gear legsGear)
		{
			PoTItemHelper.ApplyAffixes(legsGear.Item, swapItemModifier);
		}
		// missing accessories
		else if (item.damage > 0)
		{
			if (Main.LocalPlayer.inventory[0].ModItem is Gear gear)
			{
				PoTItemHelper.ApplyAffixes(gear.Item, swapItemModifier);
			}
		}
	}

	public override bool AltFunctionUse(Item item, Player player)
	{
		if (!IsGearItem(item))
		{
			return base.AltFunctionUse(item, player);
		}

		return player.GetModPlayer<AltUsePlayer>().AltFunctionAvailable;
	}
}
