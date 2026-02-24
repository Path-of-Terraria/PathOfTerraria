using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Rings.AddedLife;

public class IncarnationOfIncineration : BerylRing
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Rings/AddedLife/BerylRing";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData def = this.GetStaticData();
		def.IsUnique = true;
		def.DropChance = null;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.value = Item.buyPrice(0, 1, 0, 0);
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var igniteSpread = (ItemAffix)Affix.CreateAffix<IgniteSpreadAffix>(2, 4);
		return [igniteSpread];
	}
}
