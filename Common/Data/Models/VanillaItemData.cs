using PathOfTerraria.Common.Systems.ElementalDamage;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Data.Models;

internal class VanillaItemData()
{
	public string ItemType { get; set; }
	public Dictionary<ElementType, float> ElementProportions { get; set; }
}
