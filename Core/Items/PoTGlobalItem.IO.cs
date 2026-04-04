using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
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

		tag["type"] = (int)data.ItemType;
		tag["rarity"] = (int)data.Rarity;
		tag["influence"] = (int)data.Influence;

		tag["implicits"] = data.ImplicitCount;

		tag["ItemLevel"] = data.RealLevel;
		tag["corrupt"] = data.Corrupted;
		tag["cloned"] = data.Cloned;
		tag["namePrefix"] = (short)data.NameAffix.Prefix;
		tag["nameSuffix"] = (short)data.NameAffix.Suffix;

		List<TagCompound> affixTags = [];
		foreach (ItemAffix affix in data.Affixes)
		{
			var newTag = new TagCompound();
			affix.SaveTo(newTag);
			affixTags.Add(newTag);
		}

		tag["affixes"] = affixTags;
	}

	public override void LoadData(Item item, TagCompound tag)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		data.ItemType = (ItemType)tag.GetInt("type");
		data.Rarity = (ItemRarity)tag.GetInt("rarity");
		data.Influence = (Influence)tag.GetInt("influence");

		data.ImplicitCount = tag.GetInt("implicits");

		data.RealLevel = tag.GetInt("ItemLevel");
		data.Corrupted = tag.GetBool("corrupt");
		data.Cloned = tag.GetBool("cloned");

		sbyte prefix = (sbyte)((tag.GetShort("namePrefix") is short p && GearGlobalItem.IsPrefixValid(item, p)) ? p : -1);
		sbyte suffix = (sbyte)((tag.GetShort("nameSuffix") is short s && GearGlobalItem.IsPrefixValid(item, s)) ? s : -1);
		data.NameAffix = new((sbyte)prefix, (sbyte)suffix);

		data.Affixes.Clear();
		IList<TagCompound> affixTags = tag.GetList<TagCompound>("affixes");

		foreach (TagCompound newTag in affixTags)
		{
			if (Affix.FromTag<ItemAffix>(newTag) is ItemAffix g)
			{
				data.Affixes.Add(g);
			}
		}

		PostRoll.Invoke(item);
	}

	public override void NetSend(Item item, BinaryWriter writer)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		writer.Write((long)data.ItemType);
		writer.Write((byte)data.Rarity);
		writer.Write((byte)data.Influence);
		writer.Write((byte)data.ImplicitCount);

		writer.Write((bool)data.Corrupted);
		writer.Write((bool)data.Cloned);
		writer.Write((sbyte)data.NameAffix.Prefix);
		writer.Write((sbyte)data.NameAffix.Suffix);
		writer.Write((byte)data.RealLevel);

		writer.Write((byte)data.Affixes.Count);
		foreach (ItemAffix affix in data.Affixes)
		{
			affix.NetSend(writer);
		}
	}

	public override void NetReceive(Item item, BinaryReader reader)
	{
		PoTInstanceItemData data = item.GetInstanceData();

		data.ItemType = (ItemType)reader.ReadInt64();
		data.Rarity = (ItemRarity)reader.ReadByte();
		data.Influence = (Influence)reader.ReadByte();
		data.ImplicitCount = reader.ReadByte();

		data.Corrupted = reader.ReadBoolean();
		data.Cloned = reader.ReadBoolean();
		data.NameAffix = new(reader.ReadSByte(), reader.ReadSByte());
		data.RealLevel = reader.ReadByte();

		data.Affixes.Clear();
		int affixes = reader.ReadByte();
		for (int i = 0; i < affixes; i++)
		{
			data.Affixes.Add(Affix.FromBReader(reader));
		}

		PostRoll.Invoke(item);
	}
}
