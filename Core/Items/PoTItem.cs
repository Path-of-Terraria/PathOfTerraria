using System.Collections.Generic;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Core;

public abstract class PoTItem : ModItem
{
	// <Drop chance, item rarity, item id of item>
	internal static readonly List<Tuple<float, Rarity, int>> AllItems = [];

	/// <summary>
	/// Same as above, but should be used through <see cref="ManuallyLoadPoTItem(Mod, PoTItem)"/>.<br/>
	/// Otherwise, used to append manually-added items through <see cref="Mod.AddContent(ILoadable)"/>.
	/// </summary>
	private static readonly List<(float dropChance, int itemId)> ManuallyLoadedItems = [];

	/// <summary>
	/// Readies all types of gear to be dropped on enemy kill.
	/// </summary>
	/// <param name="pos">Where to spawn the armor</param>
	public static void GenerateItemList()
	{
		AllItems.Clear();

		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(PoTItem)) || Attribute.IsDefined(type, typeof(ManuallyLoadPoTItemAttribute)))
			{
				continue;
			}

			var instance = (PoTItem)Activator.CreateInstance(type);
			int id = ModLoader.GetMod(PathOfTerraria.ModName).Find<ModItem>(instance.Name).Type;

			if (instance.IsUnique)
			{
				AllItems.Add(new(instance.DropChance, Rarity.Unique, id));
			}
			else
			{

				if (!type.IsSubclassOf(typeof(Jewel)))
				{
					AllItems.Add(new(instance.DropChance * 0.70f, Rarity.Normal, id));
				}

				AllItems.Add(new(instance.DropChance * 0.25f, Rarity.Magic, id));
				AllItems.Add(new(instance.DropChance * 0.05f, Rarity.Rare, id));
			}
		}

		foreach ((float dropChance, int itemType) in ManuallyLoadedItems) 
		{
			AllItems.Add(new(dropChance * 0.7f, Rarity.Normal, itemType));
			AllItems.Add(new(dropChance * 0.25f, Rarity.Magic, itemType));
			AllItems.Add(new(dropChance * 0.5f, Rarity.Rare, itemType));
		}

		ManuallyLoadedItems.Clear();
	}

	public static void ManuallyLoadPoTItem(Mod mod, PoTItem instance)
	{
		mod.AddContent(instance);
		ManuallyLoadedItems.Add((instance.DropChance, instance.Type));
	}
}