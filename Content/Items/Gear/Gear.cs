using PathOfTerraria.Content.Socketables;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.ModPlayers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace PathOfTerraria.Content.Items.Gear;

internal abstract class Gear : PoTItem
{
	private Socketable[] _sockets = []; // [new Imps()];
	private int _selectedSocket = 0;

	public override void InsertAdditionalTooltipLines(List<TooltipLine> tooltips, EntityModifier thisItemModifier)
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

	public sealed override void ExtraRolls()
	{
		_selectedSocket = 0;
		int maxSockets = Rarity switch // what to do if we roll less sockets than what we have equipped?
									   // maby just not allow to roll if we have any sockets?
		{
			Rarity.Normal => Main.rand.Next(2),
			Rarity.Magic => Main.rand.Next(1, 2),
			Rarity.Rare => Main.rand.Next(1, 3),
			_ => 0,
		};

		_sockets = new Socketable[maxSockets];
	}
	public sealed override void ExtraUpdateEquips(Player player)
	{
		_sockets.Where(s => s is not null).ToList().ForEach(s => s.UpdateEquip(player, Item));
	}

	public sealed override void SwapItemModifiers(EntityModifier SawpItemModifier)
	{
		if (Item.headSlot >= 0 && Main.LocalPlayer.armor[0].active)
		{
			(Main.LocalPlayer.armor[0].ModItem as Gear).ApplyAffixes(SawpItemModifier);
		}
		else if (Item.bodySlot >= 0 && Main.LocalPlayer.armor[1].active)
		{
			(Main.LocalPlayer.armor[1].ModItem as Gear).ApplyAffixes(SawpItemModifier);
		}
		else if (Item.legSlot >= 0 && Main.LocalPlayer.armor[2].active)
		{
			(Main.LocalPlayer.armor[2].ModItem as Gear).ApplyAffixes(SawpItemModifier);
		}
		// missing accessories
		else
		{
			if (Main.LocalPlayer.inventory[0].ModItem is Gear)
			{
				(Main.LocalPlayer.inventory[0].ModItem as Gear).ApplyAffixes(SawpItemModifier);
			}
		}
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
}