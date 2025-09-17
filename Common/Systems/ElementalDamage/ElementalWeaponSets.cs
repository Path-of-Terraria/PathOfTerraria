using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

/// <summary>
/// Stores information loaded from JSON files for item elemental affinity.<br/>
/// This automatically applies elemental damage to projectiles, more TBD.
/// </summary>
[ReinitializeDuringResizeArrays]
internal class ElementalWeaponSets
{
	public static readonly Dictionary<int, Dictionary<ElementType, float>> WeaponElementProportionsById = new(ItemLoader.ItemCount);

	/// <summary>
	/// Whether this item has any elemental associations.
	/// </summary>
	/// <param name="itemId">The item ID to reference.</param>
	/// <returns>Whether the item has any innate elemental effects.</returns>
	public static bool HasElements(int itemId)
	{
		if (WeaponElementProportionsById.TryGetValue(itemId, out Dictionary<ElementType, float> value))
		{
			foreach (float proportion in value.Values)
			{
				if (proportion > 0)
				{
					return true;
				}
			}
		}

		return false;
	}

	public static bool GetElementalProportions(int itemId, out Dictionary<ElementType, float> value)
	{
		return WeaponElementProportionsById.TryGetValue(itemId, out value);
	}

	public static float GetElementStrength(int itemId, ElementType type)
	{
		if (WeaponElementProportionsById.TryGetValue(itemId, out Dictionary<ElementType, float> value))
		{
			return value[type];
		}

		return 0;
	}
}
