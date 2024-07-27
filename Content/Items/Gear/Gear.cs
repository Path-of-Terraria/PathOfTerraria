using PathOfTerraria.Content.Socketables;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using PathOfTerraria.Common.Systems.Affixes;

namespace PathOfTerraria.Content.Items.Gear;

public abstract class Gear : ModItem, ExtraRolls.IItem, GenerateAffixes.IItem, GenerateImplicits.IItem, GeneratePrefix.IItem, GenerateSuffix.IItem, InsertAdditionalTooltipLines.IItem, PostRoll.IItem, SwapItemModifiers.IItem
{
	protected virtual string GearLocalizationCategory => GetType().Name;

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
	public virtual string GeneratePrefix(string defaultPrefix)
	{
		return Language.SelectRandom((key, _) => BasicAffixSearchFilter(key, true)).Value;
	}

	/// <summary>
	/// Selects a suffix to be added to the name of the item from the provided Suffixes in localization files
	/// </summary>
	/// <returns></returns>
	public virtual string GenerateSuffix(string defaultSuffix)
	{
		return Language.SelectRandom((key, _) => BasicAffixSearchFilter(key, false)).Value;
	}

	private bool BasicAffixSearchFilter(string key, bool isPrefix)
	{
		return key.StartsWith("Mods.PathOfTerraria.Gear." + GearLocalizationCategory + (isPrefix ? ".Prefixes" : ".Suffixes"));
	}

	public virtual List<ItemAffix> GenerateAffixes()
	{
		return [];
	}

	public virtual List<ItemAffix> GenerateImplicits()
	{
		return [];
	}

	public virtual void PostRoll() { }
}