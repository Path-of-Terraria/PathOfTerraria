﻿using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.ModLoader.IO;
using PathOfTerraria.Common.Systems.ModPlayers;
using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Linq;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public abstract class Map : ModItem, GenerateName.IItem, GenerateAffixes.IItem, GenerateImplicits.IItem, IPoTGlobalItem, GetItemLevel.IItem, SetItemLevel.IItem
{
	protected sealed override bool CloneNewInstances => true;

	public abstract int MaxUses { get; }
	public virtual int WorldTier => WorldLevelBasedOnTier(Tier);

	public int RemainingUses = 0;

	internal int Tier = 1;
	internal int ItemLevel = 1;

	public override ModItem Clone(Item newEntity)
	{
		var map = base.Clone(newEntity) as Map;
		map.Tier = Tier;
		map.ItemLevel = ItemLevel;
		return map;
	}

	public override void SetDefaults() 
	{
		base.SetDefaults();

		Item.width = 32;
		Item.height = 32;
		Item.maxStack = 1;
		Item.consumable = false;
		Item.rare = ItemRarityID.Green;
		Item.value = 1000;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Map;
	}

	public virtual ushort GetTileAt(int x, int y) { return TileID.Stone; }

	public virtual void OpenMap()
	{
		OpenMapInternal();

		if (SubworldSystem.Current is MappingWorld map)
		{
			map.AreaLevel = WorldTier;
			map.Affixes = [];
			map.Affixes.AddRange(this.GetInstanceData().Affixes.Where(x => x is MapAffix).Select(x => (MapAffix)x));
		}
	}

	protected abstract void OpenMapInternal();

	/// <summary>
	/// Gets name and what tier the map is of as a singular string.
	/// </summary>
	public virtual string GetNameAndTier()
	{
		return Core.Items.GenerateName.Invoke(Item);
	}

	public static void SpawnItem(Vector2 pos)
	{
		SpawnMap<LowTierMap>(pos);
		SpawnMap<CaveMap>(pos);
	}
	
	public static void SpawnMap<T>(Vector2 pos) where T : Map
	{
		var item = new Item();
		item.SetDefaults(ModContent.ItemType<T>());
		(item.ModItem as Map).Tier = 1;

		Item.NewItem(null, pos, Vector2.Zero, item);
	}
	
	public override void SaveData(TagCompound tag)
	{
		tag.Add("usesLeft", (byte)RemainingUses);
		tag.Add("tier", (short)Tier);
	}

	public override void LoadData(TagCompound tag)
	{
		RemainingUses = tag.GetByte("usesLeft");
		Tier = tag.GetShort("tier");
	}

	public abstract string GenerateName(string defaultName);

	public static int WorldLevelBasedOnTier(int tier)
	{
		return Math.Clamp(50 + tier * 2, 50, 72);
	}

	public static int TierBasedOnWorldLevel(int area)
	{
		if (area == 0)
		{
			return 0;
		}

		// area is adjusted by 48 instead of 50 to increase the level by 1 per stage;
		// i.e. a level 50 area gives a tier 1 map, which gives a level 52 area...
		return Math.Clamp((area - 48) / 2, 0, 11);
	}

	/// <summary>
	/// Determines how many times a boss domain map can be used. This means the following:<br/>
	/// <c>1 Player:</c> 6 uses<br/>
	/// <c>2 Players:</c> 3 uses per player<br/>
	/// <c>3 Players:</c> 2 uses per player:<br/>
	/// <c>4+ Players:</c> 1 use per player.<br/>
	/// </summary>
	/// <returns></returns>
	public static int GetBossUseCount()
	{
		int def = 6 / BossDomainPlayer.GetLivesPerPlayer();

		if (Main.CurrentFrameFlags.ActivePlayersCount > 6)
		{
			def = Main.CurrentFrameFlags.ActivePlayersCount;
		}

		return def;
	}

	public List<ItemAffix> GenerateImplicits()
	{
		return [];
	}

	public List<ItemAffix> GenerateAffixes()
	{
		return [];
	}

	int GetItemLevel.IItem.GetItemLevel(int realLevel)
	{
		return ItemLevel;
	}

	void SetItemLevel.IItem.SetItemLevel(int level, ref int realLevel)
	{
		// Randomize level a bit if it's == to world level
		// TODO: Find a better way to do this?
		if (level == PoTItemHelper.PickItemLevel() && level > 50)
		{
			level = Main.rand.Next(50, level + 1);
		}

		realLevel = level;
		ItemLevel = realLevel;
		Tier = TierBasedOnWorldLevel(ItemLevel);
	}
}