using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public abstract class Map : ModItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Consumables/Maps/Map";
	private int _tier;
	private readonly List<MapAffix> _affixes = [];

	public override void SetDefaults() {
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
	}

	public virtual ushort GetTileAt(int x, int y) { return TileID.Stone;  }

    /// <summary>
    /// Allows you to customize what this item's name can be
    /// </summary>
    public virtual string GenerateName()
    {
        return "Unnamed Item";
    }

    /// <summary>
    /// Gets name and what tier the map is of as a singular string.
    /// </summary>
    public virtual string GetNameAndTier()
	{
		return GenerateName() + ": " + Tier;
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
	
	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		tooltips.Clear();
		var nameLine = new TooltipLine(Mod, "Name", Name);
		tooltips.Add(nameLine);
		
		var mapLine = new TooltipLine(Mod, "Map", "Map");
		tooltips.Add(mapLine);

		var rareLine = new TooltipLine(Mod, "Tier", "Tier: " + _tier);
		tooltips.Add(rareLine);

		foreach (MapAffix affix in _affixes)
		{
			string text = $"[i:{ItemID.MusketBall}] " + affix.GetTooltip(this);

			var affixLine = new TooltipLine(Mod, $"Affix{affix.GetHashCode()}", text);
			tooltips.Add(affixLine);
		}
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
			map.GenerateName();
		}

		Item.NewItem(null, pos, Vector2.Zero, item);
	}
	
	public override void SaveData(TagCompound tag)
	{
		tag["tier"] = _tier;

		List<TagCompound> affixTags = [];
		foreach (MapAffix affix in _affixes)
		{
			var newTag = new TagCompound();
			affix.Save(newTag);
			affixTags.Add(newTag);
		}

		tag["affixes"] = affixTags;
	}

	public override void LoadData(TagCompound tag)
	{
		_tier = tag.GetInt("tier");

		IList<TagCompound> affixTags = tag.GetList<TagCompound>("affixes");

		foreach (TagCompound newTag in affixTags)
		{
			_affixes.Add(MapAffix.FromTag(newTag));
		}
	}
}