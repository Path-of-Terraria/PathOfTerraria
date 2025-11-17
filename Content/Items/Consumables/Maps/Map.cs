using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public abstract class Map : ModItem, GenerateNameAffixes.IItem, GenerateAffixes.IItem, GenerateImplicits.IItem, IPoTGlobalItem, GetItemLevel.IItem, SetItemLevel.IItem
{
	protected sealed override bool CloneNewInstances => true;

	public abstract int MaxUses { get; }
	public abstract bool CanDrop { get; }
	public virtual int WorldLevel => WorldLevelBasedOnTier(Tier);

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
		List<MapAffix> collection = [.. this.GetInstanceData().Affixes.Where(x => x is MapAffix).Select(x => (MapAffix)x)];

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			MappingWorld.AreaLevel = WorldLevel;
			MappingWorld.MapTier = Tier;
			MappingWorld.Affixes = [];
			MappingWorld.Affixes.AddRange(collection);
		}
		else
		{
			ModContent.GetInstance<SendMappingDomainInfoHandler>().Send((short)WorldLevel, (short)Tier, collection);
		}

		OpenMapInternal();
	}

	protected abstract void OpenMapInternal();

	public virtual int GetMapTier(int itemLevel)
	{
		return TierBasedOnWorldLevel(itemLevel);
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
		tag.Add("tier", (short)Tier);
	}
	public override void LoadData(TagCompound tag)
	{
		Tier = tag.GetShort("tier");
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((short)Tier);
	}
	public override void NetReceive(BinaryReader reader)
	{
		Tier = reader.ReadInt16();
	}

	public abstract string GenerateName(string defaultName);

	public virtual void ModifyCorruptionAffixes(WeightedRandom<ItemAffix> affixes)
	{
	}

	public static int WorldLevelBasedOnTier(int tier)
	{
		return Math.Clamp(55 + tier * 2, 57, 77);
	}

	public static int TierBasedOnWorldLevel(int area)
	{
		if (area < 25) 
		{
			return 0;
		}

		// Return 1 if we're post-WoF but below map levels
		if (area >= 25 && area <= 56)
		{
			return 1;
		}

		// Calculate tier based on new progression: 57, 59, 61, 63, etc.
		// area is adjusted by 55 to align with the new tier system
		return Math.Clamp((area - 55) / 2, 1, 11);

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
		int def = 6 / BossDomainLivesPlayer.GetLivesPerPlayer(Main.CurrentFrameFlags.ActivePlayersCount);

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
		//You can find maps 1 tier lower, on your current tier, and 1 tier higher with the below. 
		//So if youre in tier 2, you can find tier 1 maps, tier 2 maps, and tier 3 maps.
		if (level == PoTItemHelper.PickItemLevel() && level >= 50)
		{
			level = Main.rand.Next(level - 3, level + 1);
		}

		realLevel = level;
		ItemLevel = realLevel;
		Tier = GetMapTier(ItemLevel);
	}

	(sbyte, sbyte) GenerateNameAffixes.IItem.GenerateAffixIds()
	{
		return (-1, -1);
	}
}