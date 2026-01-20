using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Common.Tiles;

/// <summary> Automatically generates an item that places the given <see cref="ModTile"/> down.<br/>
/// The <see cref="StaticItemDefaults"/>, <see cref="SetItemDefaults"/> and <see cref="AddItemRecipes"/> hooks can be used to conveniently modify the generated item.<br/><br/>
/// Copied from the same implementation in Spirit Reforged: https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/TileCommon/IAutoloadTileItem.cs<br/>
/// Helpers: https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/ItemCommon/AutoContent.cs</summary>
public interface IAutoloadTileItem
{
	// These are already defined on ModTiles and shortens the autoloading code a bit.
	public string Name { get; }
	public string Texture { get; }

	public void StaticItemDefaults(ModItem item) { }
	public void SetItemDefaults(ModItem item) { }
	public void AddItemRecipes(ModItem item) { }
}

public class AutoloadTileItemHandler
{
	private class AutoloadTileItemSystem : ModSystem
	{
		public override void OnModLoad()
		{
			List<ModTile> types = [.. ModContent.GetContent<ModTile>().Where(x => x is IAutoloadTileItem)];

			foreach (ModTile item in types)
			{
				AutoloadItem(item);
			}
		}
	}

	public static void AutoloadItem(ModTile item)
	{
		PoTMod.Instance.AddContent(new AutoloadedTileItem(item.Name + "Item", item.Texture + "Item", (IAutoloadTileItem)item));
	}
}

/// <summary> Represents an item autoloaded with <see cref="IAutoloadTileItem"/>. </summary>
public class AutoloadedTileItem(string name, string texture, IAutoloadTileItem hooks) : ModItem
{
	protected override bool CloneNewInstances => true;
	public override string Name => _internalName;
	public override string Texture => _texture;

	private string _internalName = name;
	private string _texture = texture;
	private IAutoloadTileItem _hooks = hooks;

	public override ModItem Clone(Item newEntity)
	{
		var item = base.Clone(newEntity) as AutoloadedTileItem;
		item._internalName = _internalName;
		item._texture = _texture;
		item._hooks = _hooks;
		return item;
	}

	public override void SetStaticDefaults()
	{
		_hooks.StaticItemDefaults(this);
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(Mod.Find<ModTile>(_internalName.Replace("Item", string.Empty)).Type);
		_hooks.SetItemDefaults(this);
	}

	public override void AddRecipes()
	{
		_hooks.AddItemRecipes(this);
	}
}

public static class AutomaticItemContent
{
	public const string Suffix = "Item";

	/// <summary> Attempts to find the autoloaded ModItem associated with the given Type. Throws exceptions on failure. </summary>
	public static ModItem ModItem<T>(string prepend = "") where T : ModType
	{
		return PoTMod.Instance.Find<ModItem>(ModContent.GetInstance<T>().Name + prepend + Suffix);
	}

	/// <inheritdoc cref="ModItem{T}(string)"/>
	public static ModItem AutoModItem<T>(string prepend = "") where T : ModType
	{
		return PoTMod.Instance.Find<ModItem>(ModContent.GetInstance<T>().Name + prepend + Suffix);
	}

	/// <summary> Attempts to find the autoloaded item associated with the given Type. Throws exceptions on failure. </summary>
	public static Item Item<T>(string prepend = "") where T : ModType
	{
		return ModItem<T>(prepend).Item;
	}

	/// <inheritdoc cref="Item{T}(string)"/>
	public static Item AutoItem<T>(string prepend = "") where T : ModType
	{
		return AutoModItem<T>(prepend).Item;
	}

	/// <summary> Attempts to find the autoloaded item type associated with the given Type. Throws exceptions on failure. </summary>
	public static int ItemType<T>(string prepend = "") where T : ModType
	{
		return ModItem<T>(prepend).Type;
	}

	/// <inheritdoc cref="ItemType{T}(string)"/>
	public static int AutoItemType<T>(string prepend = "") where T : ModType
	{
		return AutoModItem<T>(prepend).Type;
	}
}