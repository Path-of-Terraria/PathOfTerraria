using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal abstract class Map : ModItem, GetItemLevel.IItem, SetItemLevel.IItem, GenerateName.IItem
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Consumables/Maps/{GetType().Name}";

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
		Item.consumable = true;
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
}