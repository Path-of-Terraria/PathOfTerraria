using PathOfTerraria.Core.Items;
using SubworldLibrary;
using PathOfTerraria.Common.Subworlds;
using System.Linq;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using Terraria.ModLoader.IO;
using PathOfTerraria.Common.Enums;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public abstract class ExplorableMap : Map, GetItemLevel.IItem, SetItemLevel.IItem
{
	internal int Tier = 1;

	public override void SetDefaults()
	{
		base.SetDefaults();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.ExplorableMap;
	}

	public sealed override void OpenMap()
	{
		OpenMapInternal();

		if (SubworldSystem.Current is MappingWorld map)
		{
			map.Level = GetSubworldLevel(map);
			map.Affixes = [];
			map.Affixes.AddRange(this.GetInstanceData().Affixes.Where(x => x is MapAffix).Select(x => (MapAffix)x));
		}
	}

	protected virtual int GetSubworldLevel(MappingWorld map)
	{
		return Tier;
	}

	/// <summary>
	/// Gets name and what tier the map is of as a singular string.
	/// </summary>
	public override string GetNameAndTier()
	{
		return Core.Items.GenerateName.Invoke(Item) + ": " + Tier;
	}

	public override void SaveData(TagCompound tag)
	{
		base.SaveData(tag);
		tag.Add("tier", (short)Tier);
	}

	public override void LoadData(TagCompound tag)
	{
		base.LoadData(tag);
		Tier = tag.GetShort("tier");
	}

	int GetItemLevel.IItem.GetItemLevel(int realLevel)
	{
		return Tier;
	}

	void SetItemLevel.IItem.SetItemLevel(int level, ref int realLevel)
	{
		realLevel = level;
		Tier = realLevel;
	}
}