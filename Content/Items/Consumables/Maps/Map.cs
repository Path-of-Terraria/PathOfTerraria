using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.ModLoader.IO;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public abstract class Map : ModItem, GetItemLevel.IItem, SetItemLevel.IItem, GenerateName.IItem
{
	public abstract int MaxUses { get; }

	public int RemainingUses = 0;

	private int _tier;

	int GetItemLevel.IItem.GetItemLevel(int realLevel)
	{
		return _tier;
	}

	void SetItemLevel.IItem.SetItemLevel(int level, ref int realLevel)
	{
		realLevel = level;
		_tier = 1 + (int)Math.Floor(realLevel / 20f);
	}

	public override void SetDefaults() {
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

	public abstract void OpenMap();

	/// <summary>
	/// Gets name and what tier the map is of as a singular string.
	/// </summary>
	public virtual string GetNameAndTier()
	{
		return Core.Items.GenerateName.Invoke(Item) + ": " + _tier;
	}

	public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
	{
		if (line.Mod == Mod.Name && line.Name == "Name")
		{
			yOffset = -2;
			line.BaseScale = Vector2.One * 1.1f;
			return true;
		}

		if (line.Mod == Mod.Name && line.Name == "Map")
		{
			yOffset = -8;
			line.BaseScale = Vector2.One * 0.8f;
			return true;
		}

		if (line.Mod == Mod.Name && line.Name == "Tier")
		{
			yOffset = 2;
			line.BaseScale = Vector2.One * 0.8f;
			return true;
		}
		
		return true;
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
		if (item.ModItem is T map)
		{
			map._tier = 1;
		}

		Item.NewItem(null, pos, Vector2.Zero, item);
	}
	
	public override void SaveData(TagCompound tag)
	{
		tag["tier"] = _tier;
		tag.Add("usesLeft", (byte)RemainingUses);
	}

	public override void LoadData(TagCompound tag)
	{
		_tier = tag.GetInt("tier");
		RemainingUses = tag.GetByte("usesLeft");
	}

	public abstract string GenerateName(string defaultName);

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
}