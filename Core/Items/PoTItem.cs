using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.UI;
using PathOfTerraria.Core.Systems;
using System.Text.RegularExpressions;
using Terraria.ID;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.TreeSystem;
using TextCopy;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Data;
using PathOfTerraria.Data.Models;
using System.IO;
using System.Runtime.CompilerServices;

namespace PathOfTerraria.Core;

public abstract class PoTItem : ModItem
{
	// <Drop chance, item rarity, item id of item>
	internal static readonly List<Tuple<float, Rarity, int>> AllItems = [];

	/// <summary>
	/// Same as above, but should be used through <see cref="ManuallyLoadPoTItem(Mod, PoTItem)"/>.<br/>
	/// Otherwise, used to append manually-added items through <see cref="Mod.AddContent(ILoadable)"/>.
	/// </summary>
	private static readonly List<(float dropChance, int itemId)> ManuallyLoadedItems = [];

	/// <summary>
	/// Readies all types of gear to be dropped on enemy kill.
	/// </summary>
	/// <param name="pos">Where to spawn the armor</param>
	public static void GenerateItemList()
	{
		AllItems.Clear();

		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(PoTItem)) || Attribute.IsDefined(type, typeof(ManuallyLoadPoTItemAttribute)))
			{
				continue;
			}

			var instance = (PoTItem)Activator.CreateInstance(type);
			int id = ModLoader.GetMod(PathOfTerraria.ModName).Find<ModItem>(instance.Name).Type;

			if (instance.IsUnique)
			{
				AllItems.Add(new(instance.DropChance, Rarity.Unique, id));
			}
			else
			{

				if (!type.IsSubclassOf(typeof(Jewel)))
				{
					AllItems.Add(new(instance.DropChance * 0.70f, Rarity.Normal, id));
				}

				AllItems.Add(new(instance.DropChance * 0.25f, Rarity.Magic, id));
				AllItems.Add(new(instance.DropChance * 0.05f, Rarity.Rare, id));
			}
		}

		foreach ((float dropChance, int itemType) in ManuallyLoadedItems) 
		{
			AllItems.Add(new(dropChance * 0.7f, Rarity.Normal, itemType));
			AllItems.Add(new(dropChance * 0.25f, Rarity.Magic, itemType));
			AllItems.Add(new(dropChance * 0.5f, Rarity.Rare, itemType));
		}

		ManuallyLoadedItems.Clear();
	}

	public static void ManuallyLoadPoTItem(Mod mod, PoTItem instance)
	{
		mod.AddContent(instance);
		ManuallyLoadedItems.Add((instance.DropChance, instance.Type));
	}

	private const float _magicFindPowerDecrease = 100f;

	internal static float ApplyRarityModifier(float chance, float dropRarityModifier)
	{
		// this is just some arbitrary function from chat gpt, modified a little...
		// it is pretty hard to get all this down when we dont know all the items we will have n such;

		chance *= 100f; // to make it effective on <0.1; it works... ok?
		float powerDecrease = chance * (1 + dropRarityModifier / _magicFindPowerDecrease) /
		                      (1 + chance * dropRarityModifier / _magicFindPowerDecrease);
		return powerDecrease;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["type"] = (int)ItemType;
		tag["rarity"] = (int)Rarity;
		tag["influence"] = (int)Influence;

		tag["implicits"] = _implicits;

		tag["name"] = _name;
		tag["ItemLevel"] = InternalItemLevel;

		List<TagCompound> affixTags = [];
		foreach (ItemAffix affix in Affixes)
		{
			var newTag = new TagCompound();
			affix.Save(newTag);
			affixTags.Add(newTag);
		}

		tag["affixes"] = affixTags;
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)ItemType);
		writer.Write((byte)Rarity);
		writer.Write((byte)Influence);

		writer.Write((byte)_implicits);

		writer.Write(_name); // unsure if this is neccecary...
		// we should probably save the name as 'GeneratePrefix-ID (Item.Name can probably be omitted) GenerateSuffix-ID'
		writer.Write((byte)InternalItemLevel);

		writer.Write((byte)Affixes.Count);

		foreach (ItemAffix affix in Affixes)
		{
			affix.NetSend(writer);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		ItemType = (ItemType)tag.GetInt("type");
		Rarity = (Rarity)tag.GetInt("rarity");
		Influence = (Influence)tag.GetInt("influence");

		_implicits = tag.GetInt("implicits");

		_name = tag.GetString("name");
		InternalItemLevel = tag.GetInt("ItemLevel");

		IList<TagCompound> affixTags = tag.GetList<TagCompound>("affixes");

		Affixes.Clear();
		foreach (TagCompound newTag in affixTags)
		{
			ItemAffix g = Affix.FromTag<ItemAffix>(newTag);
			if (g is not null)
			{
				Affixes.Add(g);
			}
		}

		PostRoll();
	}

	public override void NetReceive(BinaryReader reader)
	{
		ItemType = (ItemType)reader.ReadByte();
		Rarity = (Rarity)reader.ReadByte();
		Influence = (Influence)reader.ReadByte();

		_implicits = reader.ReadByte();

		_name = reader.ReadString();
		InternalItemLevel = reader.ReadByte();

		int affixes = reader.ReadByte();

		Affixes.Clear();
		for (int i = 0; i <  affixes; i++)
		{
			Affixes.Add(Affix.FromBReader(reader));
		}

		PostRoll();
	}

	public virtual int GetAffixCount()
	{
		return Rarity switch
		{
			Rarity.Magic => 2,
			Rarity.Rare => Main.rand.Next(3, 5),
			_ => 0
		};
	}

	/// <summary>
	/// Selects appropriate random affixes for this item, and applies them
	/// </summary>
	public void RollAffixes()
	{
		Affixes = GenerateImplicits();

		_implicits = Affixes.Count;
		for (int i = 0; i < GetAffixCount(); i++)
		{
			ItemAffixData chosenAffix = AffixRegistry.GetRandomAffixDataByItemType(ItemType);
			if (chosenAffix is null)
			{
				continue;
			}
			
			ItemAffix affix = AffixRegistry.ConvertToItemAffix(chosenAffix);
			if (affix is null)
			{
				continue;
			}
			
			affix.Value = AffixRegistry.GetRandomAffixValue(affix, ItemLevel);
			Affixes.Add(affix);
		}

		if (IsUnique)
		{
			List<ItemAffix> uniqueItemAffixes = GenerateAffixes();
			
			foreach (ItemAffix item in uniqueItemAffixes)
			{
				item.Roll();
			}

			Affixes.AddRange(uniqueItemAffixes);
		}
	}
}