using PathOfTerraria.Content.Socketables;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Gear;

public abstract class Gear : ModItem, ICopyToClipboardItem, IExtraRolls, IGenerateAffixesItem, IGenerateImplicitsItem, IGeneratePrefixItem, IGenerateSuffixItem, IInsertAdditionalTooltipLinesItem, IPostRollItem, ISwapItemModifiersItem
{
	protected virtual string GearLocalizationCategory => GetType().Name;

	private Socketable[] _sockets = [];
	private int _selectedSocket;
	
	public override void OnCreated(ItemCreationContext context)
	{
		base.OnCreated(context);
		if (context is not RecipeItemCreationContext)
		{
			return;
		}

		PoTInstanceItemData data = this.GetInstanceData();

		data.Rarity = Rarity.Magic; //All crafted items are magic rarity
		data.Affixes.Clear();
		PoTItemHelper.Roll(Item, PoTItemHelper.PickItemLevel());
	}

	public virtual void InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier)
	{
		if (_sockets.Length > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "Space", " "));
		}

		// sockets
		for (int i = 0; i < _sockets.Length; i++)
		{
			string text = "";
			if (_sockets[i] is not null)
			{
				text = _sockets[i].GenerateName();
			}

			var affixLine = new TooltipLine(Mod, $"Socket{i}",
				$"[i:{(i == _selectedSocket ? ItemID.NanoBullet : ItemID.ChlorophyteBullet)}] " + text);
			tooltips.Add(affixLine);
		}
	}

	public virtual void ExtraRolls(Item item)
	{
		PoTInstanceItemData data = item.GetInstanceData();
		
		_selectedSocket = 0;
		int maxSockets = data.Rarity switch // what to do if we roll less sockets than what we have equipped?
									        // maby just not allow to roll if we have any sockets?
		{
			Rarity.Normal => Main.rand.Next(2),
			Rarity.Magic => Main.rand.Next(1, 2),
			Rarity.Rare => Main.rand.Next(1, 3),
			_ => 0,
		};

		_sockets = new Socketable[maxSockets];
	}

	public override void UpdateEquip(Player player)
	{
		_sockets.Where(s => s is not null).ToList().ForEach(s => s.UpdateEquip(player, Item));
	}

	public virtual void SwapItemModifiers(Item item, EntityModifier swapItemModifier)
	{
		if (Item.headSlot >= 0 && Main.LocalPlayer.armor[0].active && Main.LocalPlayer.armor[0].ModItem is Gear headGear)
		{
			PoTItemHelper.ApplyAffixes(headGear.Item, swapItemModifier);
		}
		else if (Item.bodySlot >= 0 && Main.LocalPlayer.armor[1].active && Main.LocalPlayer.armor[0].ModItem is Gear bodyGear)
		{
			PoTItemHelper.ApplyAffixes(bodyGear.Item, swapItemModifier);
		}
		else if (Item.legSlot >= 0 && Main.LocalPlayer.armor[2].active && Main.LocalPlayer.armor[0].ModItem is Gear legsGear)
		{
			PoTItemHelper.ApplyAffixes(legsGear.Item, swapItemModifier);
		}
		// missing accessories
		else if (Item.damage > 0)
		{
			if (Main.LocalPlayer.inventory[0].ModItem is Gear gear)
			{
				PoTItemHelper.ApplyAffixes(gear.Item, swapItemModifier);
			}
		}
	}
	
	public override bool AltFunctionUse(Player player)
	{
		return player.GetModPlayer<AltUsePlayer>().AltFunctionAvailable;
	}

	public void EquipItem(Player player)
	{
		if (!UseSockets())
		{
			return;
		}

		_sockets.ToList().ForEach(s => s?.OnSocket(player, Item));
	}

	public void UnEquipItem(Player player)
	{
		if (!UseSockets())
		{
			return;
		}

		_sockets.ToList().ForEach(s => s?.OnUnSocket(player, Item));
	}

	public bool IsThisItemActive(Player player)
	{
		return player.inventory[0] == Item || player.armor.Contains(Item);
	}
	
	public static bool IsThisItemActive(Player player, Item item)
	{
		return player.inventory[0] == item || player.armor.Contains(item);
	}

	public bool UseSockets()
	{
		return _sockets.Length != 0;
	}

	public void NextSocket()
	{
		_selectedSocket++;
		if (_selectedSocket >= _sockets.Length)
		{
			_selectedSocket = 0;
		}
	}
	
	public void PrevSocket()
	{
		_selectedSocket--;
		if (_selectedSocket < 0)
		{
			_selectedSocket = _sockets.Length - 1;
		}
	}

	public void Socket(Player player, Socketable item)
	{
		if (_sockets.Length == 0)
		{
			return;
		}

		if (_sockets[_selectedSocket] is not null)
		{
			_sockets[_selectedSocket].OnUnSocket(player, Item);
			Main.mouseItem = _sockets[_selectedSocket].Item;
		}
		else
		{
			Main.mouseItem = new Item();
		}

		_sockets[_selectedSocket] = item;

		if (IsThisItemActive(player))
		{
			item.OnSocket(player, Item);
		}
	}

	public void ShiftClick(Player player)
	{
		if (Main.mouseItem.active && Main.mouseItem.ModItem is Socketable)
		{
			Socket(player, Main.mouseItem.ModItem as Socketable);
		}
		else if (_sockets[_selectedSocket] is not null)
		{
			if (IsThisItemActive(player))
			{
				_sockets[_selectedSocket].OnUnSocket(player, Item);
			}

			Main.mouseItem = _sockets[_selectedSocket].Item;
			_sockets[_selectedSocket] = null;
		}
	}

	/// <summary>
	/// Selects a prefix to be added to the name of the item from the provided Prefixes in localization files
	/// </summary>
	/// <returns></returns>
	public virtual string GeneratePrefix(Item item)
	{
		string str = Language.SelectRandom((key, _) => BasicAffixSearchFilter(key, true)).Value;
		return str;
	}

	/// <summary>
	/// Selects a suffix to be added to the name of the item from the provided Suffixes in localization files
	/// </summary>
	/// <returns></returns>
	public virtual string GenerateSuffix(Item item)
	{
		return Language.SelectRandom((key, _) => BasicAffixSearchFilter(key, false)).Value;
	}

	private bool BasicAffixSearchFilter(string key, bool isPrefix)
	{
		return key.StartsWith("Mods.PathOfTerraria.Gear." + GearLocalizationCategory + (isPrefix ? ".Prefixes" : ".Suffixes"));
	}

	public override void SaveData(TagCompound tag)
	{
		base.SaveData(tag); // affixes

		tag["socketCount"] = _sockets.Length;

		for (int i = 0; i < _sockets.Length; i++)
		{
			Socketable socket = _sockets[i];
			if (socket is not null)
			{
				var newTag = new TagCompound();
				socket.Save(newTag);
				tag.Add("Socket" + i, newTag);
			}
		}
	}

	public override void LoadData(TagCompound tag)
	{
		base.LoadData(tag); // affixes

		int socketCount = tag.GetInt("socketCount");
		_sockets = new Socketable[socketCount];

		for (int i = 0; i < _sockets.Length; i++)
		{
			if (tag.TryGet("Socket" + i, out TagCompound newTag))
			{
				Socketable g = Socketable.FromTag(newTag);
				if (g is not null)
				{
					_sockets[i] = g;
				}
			}
		}
	}

	public virtual List<ItemAffix> GenerateAffixes(Item item)
	{
		return [];
	}

	public virtual List<ItemAffix> GenerateImplicits(Item item)
	{
		return [];
	}

	public virtual void PostRoll(Item item)
	{
	}
}