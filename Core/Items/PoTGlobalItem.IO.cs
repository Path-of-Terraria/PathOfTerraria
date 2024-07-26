using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems.Affixes;
using Stubble.Core.Classes;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Items;

// Handles saving and loading data and networking.

partial class PoTGlobalItem : GlobalItem
{
	public override void SaveData(Item item, TagCompound tag)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		tag["type"] = data.ItemType;
		tag["rarity"] = data.Rarity;
		tag["influence"] = data.Influence;

		tag["implicits"] = data.ImplicitCount;

		tag["name"] = data.SpecialName;
		tag["ItemLevel"] = IItemLevelControllerItem.GetLevel(item);

		List<TagCompound> affixTags = [];
		foreach (ItemAffix affix in data.Affixes)
		{
			var newTag = new TagCompound();
			affix.Save(newTag);
			affixTags.Add(newTag);
		}

		tag["affixes"] = affixTags;
	}

	public override void LoadData(Item item, TagCompound tag)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		data.ItemType = (ItemType)tag.GetInt("type");
		data.Rarity = (Rarity)tag.GetInt("rarity");
		data.Influence = (Influence)tag.GetInt("influence");

		data.ImplicitCount = tag.GetInt("implicits");

		data.SpecialName = tag.GetString("name");
		IItemLevelControllerItem.SetLevel(item, tag.GetInt("ItemLevel"));

		data.Affixes.Clear();
		IList<TagCompound> affixTags = tag.GetList<TagCompound>("affixes");
		foreach (TagCompound newTag in affixTags)
		{
			if (Affix.FromTag<ItemAffix>(newTag) is ItemAffix g)
			{
				data.Affixes.Add(g);
			}
		}

		IPostRollItem.Invoke(item);
	}

	public override void NetSend(Item item, BinaryWriter writer)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		writer.Write((byte)data.ItemType);
		writer.Write((byte)data.Rarity);
		writer.Write((byte)data.Influence);

		writer.Write((byte)data.ImplicitCount);

		// Is this necessary?
		// Probably should save the name as
		// `GenericPrefix-ID (Item.Name can probably be omitted) GenericSuffix-ID`.
		writer.Write(data.SpecialName);
		writer.Write((byte)IItemLevelControllerItem.GetLevel(item));

		writer.Write(data.Affixes.Count);
		foreach (ItemAffix affix in data.Affixes)
		{
			affix.NetSend(writer);
		}
	}

	public override void NetReceive(Item item, BinaryReader reader)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		data.ItemType = (ItemType)reader.ReadByte();
		data.Rarity = (Rarity)reader.ReadByte();
		data.Influence = (Influence)reader.ReadByte();

		data.ImplicitCount = reader.ReadByte();

		data.SpecialName = reader.ReadString();
		IItemLevelControllerItem.SetLevel(item, reader.ReadByte());

		data.Affixes.Clear();
		int affixes = reader.ReadByte();
		for (int i = 0; i < affixes; i++)
		{
			data.Affixes.Add(Affix.FromBReader(reader));
		}

		IPostRollItem.Invoke(item);
	}
}
