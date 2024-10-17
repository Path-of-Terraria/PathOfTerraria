using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Socketables;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Core.Items;

internal sealed partial class GearGlobalItem : GlobalItem, InsertAdditionalTooltipLines.IGlobal, ExtraRolls.IGlobal, SwapItemModifiers.IGlobal, GeneratePrefix.IGlobal, GenerateSuffix.IGlobal
{
	private static HashSet<int> _optInGearItems = [];

	public static void EquipItem(Item item, Player player)
	{
		if (!UseSockets(item))
		{
			return;
		}

		foreach (Socketable socket in item.GetGearData().Sockets)
		{
			socket.OnSocket(player, item);
		}
	}

	public static void UnequipItem(Item item, Player player)
	{
		if (!UseSockets(item))
		{
			return;
		}

		foreach (Socketable socket in item.GetGearData().Sockets)
		{
			socket.OnUnsocket(player, item);
		}
	}

	public static bool UseSockets(Item item)
	{
		return IsGearItem(item) && item.GetGearData().Sockets.Length > 0;
	}

	public static void NextSocket(Item item)
	{
		GearInstanceData gearData = item.GetGearData();
		gearData.SelectedSocket++;
		if (gearData.SelectedSocket >= gearData.Sockets.Length)
		{
			gearData.SelectedSocket = 0;
		}
	}

	public static void PrevSocket(Item item)
	{
		GearInstanceData gearData = item.GetGearData();
		gearData.SelectedSocket--;
		if (gearData.SelectedSocket < 0)
		{
			gearData.SelectedSocket = gearData.Sockets.Length - 1;
		}
	}

	public static void Socket(Socketable socket, Player player)
	{
		GearInstanceData gearData = socket.Item.GetGearData();

		if (gearData.Sockets.Length == 0)
		{
			return;
		}

		if (gearData.Sockets[gearData.SelectedSocket] is not null)
		{
			gearData.Sockets[gearData.SelectedSocket].OnUnsocket(player, socket.Item);
			Main.mouseItem = gearData.Sockets[gearData.SelectedSocket].Item;
		}
		else
		{
			Main.mouseItem = new Item();
		}

		gearData.Sockets[gearData.SelectedSocket] = socket;

		if (IsActive(player, socket.Item))
		{
			socket.OnSocket(player, socket.Item);
		}
	}

	public static void ShiftClick(Item item, Player player)
	{
		if (!IsGearItem(item) && Main.mouseItem.ModItem is not Socketable)
		{
			return;
		}

		GearInstanceData gearData = item.GetGearData();
		if (Main.mouseItem.active && Main.mouseItem.ModItem is Socketable)
		{
			Socket(Main.mouseItem.ModItem as Socketable, player);
		}
		else if (gearData.Sockets[gearData.SelectedSocket] is not null)
		{
			if (IsActive(player, item))
			{
				gearData.Sockets[gearData.SelectedSocket].OnUnsocket(player, item);
			}

			Main.mouseItem = gearData.Sockets[gearData.SelectedSocket].Item;
			gearData.Sockets[gearData.SelectedSocket] = null;
		}
	}

	public override void Unload()
	{
		base.Unload();

		_optInGearItems = null;
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

	void InsertAdditionalTooltipLines.IGlobal.InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips)
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
			PoTItemHelper.ApplyAffixes(headGear.Item, swapItemModifier, Main.LocalPlayer);
		}
		else if (item.bodySlot >= 0 && Main.LocalPlayer.armor[1].active && Main.LocalPlayer.armor[0].ModItem is Gear bodyGear)
		{
			PoTItemHelper.ApplyAffixes(bodyGear.Item, swapItemModifier, Main.LocalPlayer);
		}
		else if (item.legSlot >= 0 && Main.LocalPlayer.armor[2].active && Main.LocalPlayer.armor[0].ModItem is Gear legsGear)
		{
			PoTItemHelper.ApplyAffixes(legsGear.Item, swapItemModifier, Main.LocalPlayer);
		}
		// missing accessories
		else if (item.damage > 0)
		{
			if (Main.LocalPlayer.inventory[0].ModItem is Gear gear)
			{
				PoTItemHelper.ApplyAffixes(gear.Item, swapItemModifier, Main.LocalPlayer);
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

	void GeneratePrefix.IGlobal.ModifyPrefix(Item item, ref string prefix)
	{
		prefix = Language.SelectRandom((key, _) => BasicAffixSearchFilter(item, key, true)).Value;
	}

	void GenerateSuffix.IGlobal.ModifySuffix(Item item, ref string suffix)
	{
		suffix = Language.SelectRandom((key, _) => BasicAffixSearchFilter(item, key, false)).Value;
	}

	private static bool BasicAffixSearchFilter(Item item, string key, bool isPrefix)
	{
		return key.StartsWith("Mods.PathOfTerraria.Gear." + GearLocalizationCategory.Invoke(item) + (isPrefix ? ".Prefixes" : ".Suffixes"));
	}
}
