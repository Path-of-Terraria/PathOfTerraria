using PathOfTerraria.Core;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using PathOfTerraria.Common;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal abstract class Map : PoTItem
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Items/Consumables/Maps/Map";
	private int _tier;

	public override int ItemLevel
	{
		get => _tier;
		set
		{ InternalItemLevel = value; _tier = 1 + (int)Math.Floor(InternalItemLevel / 20f); }
	}

	public override void Defaults() {
		Item.width = 32;
		Item.height = 32;
		Item.useStyle = ItemUseStyleID.DrinkLiquid;
		Item.useAnimation = 15;
		Item.useTime = 15;
		Item.useTurn = true;
		Item.UseSound = SoundID.Item3;
		Item.maxStack = 1;
		Item.consumable = true;
		Item.rare = ItemRarityID.Green;
		Item.value = 1000;

		ItemType = ItemType.Map;
	}

	public virtual ushort GetTileAt(int x, int y) { return TileID.Stone;  }

    /// <summary>
    /// Gets name and what tier the map is of as a singular string.
    /// </summary>
    public virtual string GetNameAndTier()
	{
		return GenerateName() + ": " + _tier;
	}
	
    public override bool? UseItem(Player player)
	{
		MappingSystem.EnterMap(this);
		return true;
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

		base.SaveData(tag);
	}

	public override void LoadData(TagCompound tag)
	{
		_tier = tag.GetInt("tier");

		base.LoadData(tag);
	}
}